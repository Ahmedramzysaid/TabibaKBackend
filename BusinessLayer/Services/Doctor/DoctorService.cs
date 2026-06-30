using AutoMapper;
using BusinessLayer.Validations;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Persistence;
using NetTopologySuite.Geometries;

namespace BusinessLayer.Services;

    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly DomainLayer.Interfaces.IEmailService _emailService;

        public DoctorService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, DomainLayer.Interfaces.IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
        }

    private ValidationsResult ValidateDoctor(DoctorDto doctorDto)
    {
        var validator = new DoctorValidator();
        var validationResult = validator.Validate(doctorDto);
        if (!validationResult.IsValid)
        {
            string message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new ValidationsResult(false, message);
        }

        return new ValidationsResult(true, "");
    }


    public async Task<Result<PaginatedResult<DoctorDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsAvailable && d.User != null)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
                return Result<PaginatedResult<DoctorDto>>.Failure("There are no doctors found", ServiceErrorType.NotFound);

            var doctors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var doctorsDto = _mapper.Map<IEnumerable<DoctorDto>>(doctors);
            var paged = PaginatedResult<DoctorDto>.Create(doctorsDto, totalCount, pageNumber, pageSize);
            return Result<PaginatedResult<DoctorDto>>.Success(paged);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException != null
                ? $"{ex.Message} | Inner: {ex.InnerException.Message}"
                : ex.Message;
            return Result<PaginatedResult<DoctorDto>>.Failure(message, ServiceErrorType.ServerError);
        }
    }

    public async Task<Result<DoctorDto>> GetById(string id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
            return Result<DoctorDto>.Failure("Invalid doctor id, the doctor with this id is not found",
             ServiceErrorType.NotFound);

        var doctorDto = _mapper.Map<DoctorDto>(doctor);
        return Result<DoctorDto>.Success(doctorDto);
    }

    public async Task<Result<IEnumerable<DoctorDto>>> GetBySpecialization(string specialization)
    {
        if (string.IsNullOrWhiteSpace(specialization))
        {
            return Result<IEnumerable<DoctorDto>>.Failure("Specialization is required", ServiceErrorType.ValidationError);
        }

        var doctors = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .Where(d => d.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();

        if (doctors != null && doctors.Any())
        {
            var doctorsDto = _mapper.Map<IEnumerable<DoctorDto>>(doctors);
            return Result<IEnumerable<DoctorDto>>.Success(doctorsDto);
        }

        return Result<IEnumerable<DoctorDto>>.Failure($"No doctors found with specialization: {specialization}", ServiceErrorType.NotFound);
    }

    private async Task<Result<DoctorDto>> ValidateNewDoctor(DoctorDto doctorDto)
    {
        var validations = ValidateDoctor(doctorDto);
        if (!validations.IsValid)
            return Result<DoctorDto>.Failure(validations.ErrorMessage, ServiceErrorType.ValidationError);


        return Result<DoctorDto>.Success();
    }

    public async Task<Result<DoctorDto>> Add(DoctorDto doctorDto, string password)
    {
        var validateNewDoctor = await ValidateNewDoctor(doctorDto);
        if (!validateNewDoctor.IsSuccess)
            return validateNewDoctor;

        if (!string.IsNullOrEmpty(doctorDto.Id))
        {
            var existingDoctor = await _unitOfWork.Doctors.GetById(doctorDto.Id);
            if (existingDoctor != null)
            {
                return Result<DoctorDto>.Failure("Doctor with this ID already exists. Please leave ID empty for new doctors.", ServiceErrorType.ValidationError);
            }
        }

        var email = doctorDto.Email?.Trim() ?? "";
        if (string.IsNullOrEmpty(email))
            return Result<DoctorDto>.Failure("Email is required.", ServiceErrorType.ValidationError);

        var existingEmailUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingEmailUser != null)
        {
            return Result<DoctorDto>.Failure("User with this email already exists. Please use a different email.", ServiceErrorType.ValidationError);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Result<DoctorDto>.Failure("Password is required", ServiceErrorType.ValidationError);
        }

        var userName = email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);

        var applicationUser = new ApplicationUser
        {
            FullName = doctorDto.FullName,
            Email = email,
            UserName = userName,
            EmailConfirmed = true,
            DateOfBirth = doctorDto.DateOfBirth ?? DateTime.UtcNow,
            Gender = doctorDto.Gender,
            Latitude = doctorDto.Latitude ?? 0,
            Longitude = doctorDto.Longitude ?? 0,
            DateOfRegistration = doctorDto.DateOfRegistration != default ? doctorDto.DateOfRegistration : DateTime.UtcNow,
            ProfileImageUrl = doctorDto.ProfileImageUrl
        };

        SyncLocation(applicationUser);

        var createUserResult = await _userManager.CreateAsync(applicationUser, password);

        if (!createUserResult.Succeeded)
        {
            var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
            return Result<DoctorDto>.Failure($"Failed to create user: {errors}", ServiceErrorType.ValidationError);
        }

        try
        {
            var doctorRole = await _roleManager.FindByNameAsync(Roles.Doctor);
            if (doctorRole == null)
            {
                doctorRole = new IdentityRole(Roles.Doctor)
                {
                    NormalizedName = Roles.Doctor.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                var createRoleResult = await _roleManager.CreateAsync(doctorRole);
                if (!createRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
                }
            }

            if (doctorRole != null)
            {
                var existingRoles = await _userManager.GetRolesAsync(applicationUser);
                if (!existingRoles.Contains(Roles.Doctor))
                {
                    var addToRoleResult = await _userManager.AddToRoleAsync(applicationUser, Roles.Doctor);
                    if (!addToRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        var doctor = new Doctor
        {
            Id = applicationUser.Id, // Same ID as ApplicationUser (auto-generated)
            Specialization = doctorDto.Specialization,
            IdNo = doctorDto.IdNo,
            IdImageFrontUrl = doctorDto.IdImageFrontUrl,
            IdImageBackUrl = doctorDto.IdImageBackUrl,
            Price = doctorDto.Price,
            IsAvailable = doctorDto.IsAvailable
        };

        await _unitOfWork.Doctors.Add(doctor);
        var saved = await _unitOfWork.SaveChanges();

        if (!saved)
        {
            await _userManager.DeleteAsync(applicationUser);
            return Result<DoctorDto>.Failure("Failed to add new doctor in database", ServiceErrorType.DatabaseError);
        }

        var createdDoctor = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctor.Id);

        if (createdDoctor == null)
        {
            await _userManager.DeleteAsync(applicationUser);
            return Result<DoctorDto>.Failure("Failed to retrieve created doctor", ServiceErrorType.DatabaseError);
        }

        var doctorEmail = createdDoctor?.User?.Email ?? applicationUser.Email;
        if (!string.IsNullOrWhiteSpace(doctorEmail))
        {
            var body = EmailTemplates.WelcomeDoctor(applicationUser.FullName ?? applicationUser.UserName);
            _ = _emailService.SendEmailAsync(doctorEmail.Trim(), "Welcome to Tabibak", body, isHtml: true);
        }

        var createdDoctorDto = _mapper.Map<DoctorDto>(createdDoctor);
        return Result<DoctorDto>.Success(createdDoctorDto);
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }


    public async Task<Result<DoctorDto>> Update(DoctorDto doctorDto)
    {
        var validations = ValidateDoctor(doctorDto);
        if (!validations.IsValid)
            return Result<DoctorDto>.Failure(validations.ErrorMessage, ServiceErrorType.ValidationError);

        var updatedDoctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorDto.Id);
            
        if (updatedDoctor is null)
            return Result<DoctorDto>.Failure("Doctor is not found to update", ServiceErrorType.NotFound);

        if (!string.IsNullOrWhiteSpace(doctorDto.Specialization))
        {
            updatedDoctor.Specialization = doctorDto.Specialization;
        }

        if (!string.IsNullOrWhiteSpace(doctorDto.IdNo))
        {
            updatedDoctor.IdNo = doctorDto.IdNo;
        }

        if (doctorDto.IdImageFrontUrl != null)
        {
            updatedDoctor.IdImageFrontUrl = doctorDto.IdImageFrontUrl;
        }

        if (doctorDto.IdImageBackUrl != null)
        {
            updatedDoctor.IdImageBackUrl = doctorDto.IdImageBackUrl;
        }

        if (doctorDto.Price.HasValue)
        {
            updatedDoctor.Price = doctorDto.Price;
        }

        updatedDoctor.IsAvailable = doctorDto.IsAvailable;

        if (updatedDoctor.User != null)
        {
            if (!string.IsNullOrWhiteSpace(doctorDto.FullName))
            {
                updatedDoctor.User.FullName = doctorDto.FullName;
            }

            if (doctorDto.DateOfBirth.HasValue)
            {
                updatedDoctor.User.DateOfBirth = doctorDto.DateOfBirth.Value;
            }

            if (!string.IsNullOrWhiteSpace(doctorDto.Gender))
            {
                updatedDoctor.User.Gender = doctorDto.Gender;
            }

            if (doctorDto.Latitude.HasValue)
            {
                updatedDoctor.User.Latitude = doctorDto.Latitude.Value;
            }

            if (doctorDto.Longitude.HasValue)
            {
                updatedDoctor.User.Longitude = doctorDto.Longitude.Value;
            }

            SyncLocation(updatedDoctor.User);

            if (doctorDto.ProfileImageUrl != null)
            {
                updatedDoctor.User.ProfileImageUrl = doctorDto.ProfileImageUrl;
            }
            
            if (string.IsNullOrWhiteSpace(doctorDto.Email))
            {
                return Result<DoctorDto>.Failure("Email is required.", ServiceErrorType.ValidationError);
            }
            var newEmail = doctorDto.Email.Trim();
            if (updatedDoctor.User.Email != newEmail)
            {
                var existingByEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == newEmail);
                if (existingByEmail != null && existingByEmail.Id != updatedDoctor.Id)
                {
                    return Result<DoctorDto>.Failure("User with this email already exists. Please use a different email.", ServiceErrorType.ValidationError);
                }
                var emailChangeResult = await _userManager.SetEmailAsync(updatedDoctor.User, newEmail);
                if (!emailChangeResult.Succeeded)
                {
                    return Result<DoctorDto>.Failure($"Failed to update email: {string.Join(", ", emailChangeResult.Errors.Select(e => e.Description))}", ServiceErrorType.ValidationError);
                }
            }
        }

        var result = await _unitOfWork.SaveChanges();

        if (!result)
        {
            return Result<DoctorDto>.Failure("Failed to update the doctor", ServiceErrorType.DatabaseError);
        }

        return Result<DoctorDto>.Success();
    }

    public async Task<Result<DoctorDto>> Delete(string id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
            return Result<DoctorDto>.Failure($"There is no doctor with id = {id}", ServiceErrorType.NotFound);

        var user = doctor.User;

        await _unitOfWork.DoctorRatings.Delete(r => r.DoctorID == id);
        await _unitOfWork.Appointments.Delete(a => a.DoctorID == id);

        _unitOfWork.Doctors.Delete(doctor);
        var doctorDeleted = await _unitOfWork.SaveChanges();

        if (!doctorDeleted)
            return Result<DoctorDto>.Failure("Some error occurred during deleting doctor in database.", ServiceErrorType.DatabaseError);

        if (user != null)
        {
            var deleteUserResult = await _userManager.DeleteAsync(user);
            if (!deleteUserResult.Succeeded)
            {
                var errors = string.Join(", ", deleteUserResult.Errors.Select(e => e.Description));
                return Result<DoctorDto>.Failure($"Doctor record deleted but failed to delete user: {errors}", ServiceErrorType.DatabaseError);
            }
        }

        return Result<DoctorDto>.Success();
    }

    public async Task<Result<IEnumerable<string>>> GetAllSpecializations()
    {
        var specializations = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrEmpty(d.Specialization))
            .Select(d => d.Specialization)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        if (!specializations.Any())
            return Result<IEnumerable<string>>.Failure("No specializations found", ServiceErrorType.NotFound);

        return Result<IEnumerable<string>>.Success(specializations);
    }

    public async Task<Result<IEnumerable<DoctorSearchDto>>> SearchDoctors(string? specialization = null, string? name = null)
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .Where(d => 
                (string.IsNullOrEmpty(specialization) || d.Specialization.Contains(specialization)) &&
                (string.IsNullOrEmpty(name) || d.User.FullName.Contains(name))
            )
            .ToListAsync();

        if (!doctors.Any())
            return Result<IEnumerable<DoctorSearchDto>>.Failure("No doctors found matching the criteria", ServiceErrorType.NotFound);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .ToListAsync();
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var doctorSearchDtos = doctors.Select(d =>
        {
            var doctorAppointments = appointments.Where(a => a.DoctorID == d.Id).ToList();
            var upcomingToday = doctorAppointments
                .Where(a => a.AppointmentDate == today &&
                           a.AppointmentStatus == (short)AppointmentStatus.Pending)
                .ToList();
            
            return new DoctorSearchDto
            {
                Id = d.Id,
                FullName = d.User?.FullName ?? "Unknown",
                Specialization = d.Specialization,
                ProfileImageUrl = d.User?.ProfileImageUrl,
                Email = d.User?.Email,
                TotalAppointments = doctorAppointments.Count,
                IsAvailable = !upcomingToday.Any()
            };
        }).ToList();

        return Result<IEnumerable<DoctorSearchDto>>.Success(doctorSearchDtos);
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStats(string? doctorId = null)
    {
        var appointmentsQuery = _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Payment)
            .AsNoTracking();

        if (doctorId != null)
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorID == doctorId);

        var appointments = await appointmentsQuery.ToListAsync();
        var patients = await _context.Patients.AsNoTracking().CountAsync();
        var doctors = await _context.Doctors.AsNoTracking().CountAsync();
        var payments = await _context.Payments.AsNoTracking().ToListAsync();

        var now = DateTime.UtcNow;
        var paymentToday = now.Date;

        var stats = new DashboardStatsDto
        {
            TotalAppointments = appointments.Count,
            PendingAppointments = appointments.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Pending),
            CompletedAppointments = appointments.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Completed),
            CanceledAppointments = appointments.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Canceled),
            TotalPatients = patients,
            TotalDoctors = doctors,
            TotalRevenue = (decimal)payments.Sum(p => p.AmountPaid),
            TodayRevenue = (decimal)payments.Where(p => p.PaymentDate.Date == paymentToday).Sum(p => p.AmountPaid)
        };

        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;
        var upcoming = appointments
            .Where(a => a.AppointmentStatus != (short)AppointmentStatus.Canceled
                        && (a.AppointmentDate > today
                            || (a.AppointmentDate == today && a.AppointmentTime >= nowTime)))
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .Take(5)
            .ToList();

        stats.UpcomingAppointments = upcoming.Select(a => new AppointmentWithDetailsDto
        {
            AppointmentID = a.AppointmentID,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime,
            AppointmentStatus = a.AppointmentStatus,
            StatusName = a.AppointmentStatus switch
            {
                (short)AppointmentStatus.Pending => "Pending",
                (short)AppointmentStatus.Rescheduled => "Rescheduled",
                (short)AppointmentStatus.Completed => "Completed",
                (short)AppointmentStatus.Canceled => "Canceled",
                _ => "Unknown"
            },
            PatientID = a.PatientID,
            PatientName = a.Patient?.User?.FullName ?? "Unknown",
            DoctorID = a.DoctorID,
            DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
            DoctorSpecialization = a.Doctor?.Specialization ?? ""
        }).ToList();

        var recent = appointments
            .Where(a => a.AppointmentStatus == (short)AppointmentStatus.Completed)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .Take(5)
            .ToList();

        stats.RecentAppointments = recent.Select(a => new AppointmentWithDetailsDto
        {
            AppointmentID = a.AppointmentID,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime,
            AppointmentStatus = a.AppointmentStatus,
            StatusName = "Completed",
            PatientID = a.PatientID,
            PatientName = a.Patient?.User?.FullName ?? "Unknown",
            DoctorID = a.DoctorID,
            DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
            DoctorSpecialization = a.Doctor?.Specialization ?? "",
            AmountPaid = a.Payment != null ? (decimal?)a.Payment.AmountPaid : null,
            PaymentDate = a.Payment?.PaymentDate
        }).ToList();

        return Result<DashboardStatsDto>.Success(stats);
    }


    private const double RatingWeight = 0.4;
    private const double DistanceWeight = 0.6;
    private const double MaxRating = 5.0;

    public async Task<Result<IEnumerable<NearbyDoctorDto>>> GetNearbyBySpecialization(
        double latitude, double longitude, string specialization, double maxDistanceKm = 50, string sortBy = "score")
    {
        if (string.IsNullOrWhiteSpace(specialization))
            return Result<IEnumerable<NearbyDoctorDto>>.Failure("Specialization is required", ServiceErrorType.ValidationError);

        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var queryPoint = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(longitude, latitude));

        var maxDistanceMeters = maxDistanceKm * 1000;

        var doctors = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .Where(d => d.IsAvailable
                     && d.User != null
                     && d.User.Location != null
                     && d.User.Location.Distance(queryPoint) <= maxDistanceMeters
                     && d.Specialization.ToLower().Contains(specialization.ToLower()))
            .Select(d => new
            {
                Doctor = d,
                DistanceMeters = d.User!.Location!.Distance(queryPoint)
            })
            .ToListAsync();

        if (!doctors.Any())
            return Result<IEnumerable<NearbyDoctorDto>>.Failure(
                $"No available doctors found with specialization '{specialization}' within {maxDistanceKm} km",
                ServiceErrorType.NotFound);

        var results = doctors.Select(x =>
        {
            var distanceKm = x.DistanceMeters / 1000.0;
            var normalizedRating = (double)(x.Doctor.Rating ?? 0m) / MaxRating;
            var normalizedProximity = 1.0 - (distanceKm / maxDistanceKm);
            var score = (RatingWeight * normalizedRating) + (DistanceWeight * normalizedProximity);

            return new NearbyDoctorDto
            {
                Id = x.Doctor.Id,
                FullName = x.Doctor.User!.FullName ?? "Unknown",
                Specialization = x.Doctor.Specialization,
                ProfileImageUrl = x.Doctor.User.ProfileImageUrl,
                Price = x.Doctor.Price,
                Rating = x.Doctor.Rating,
                RatingCount = x.Doctor.RatingCount,
                DistanceKm = Math.Round(distanceKm, 2),
                Score = Math.Round(score, 4),
                Latitude = x.Doctor.User.Latitude,
                Longitude = x.Doctor.User.Longitude
            };
        }).ToList();

        results = sortBy?.ToLower() switch
        {
            "rating" => results.OrderByDescending(d => d.Rating ?? 0).ThenBy(d => d.DistanceKm).ToList(),
            _ => results.OrderByDescending(d => d.Score).ToList()  // default: combined score
        };

        return Result<IEnumerable<NearbyDoctorDto>>.Success(results);
    }

    private static void SyncLocation(ApplicationUser user)
    {
        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        user.Location = gf.CreatePoint(new Coordinate(user.Longitude, user.Latitude));
    }
}
