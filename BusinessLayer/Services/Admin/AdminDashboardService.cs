using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccessLayer.Persistence;
using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminDashboardService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<AdminSystemOverviewDto>> GetSystemOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalUsers = await _context.Users.CountAsync();
            var totalDoctors = await _context.Doctors.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            
            var appointmentsQuery = _context.Appointments.AsQueryable();
            var totalAppointments = await appointmentsQuery.CountAsync();
            var totalCompletedAppointments = await appointmentsQuery.CountAsync(a => a.AppointmentStatus == (short)AppointmentStatus.Completed);
            var totalPendingAppointments = await appointmentsQuery.CountAsync(a => a.AppointmentStatus == (short)AppointmentStatus.Pending);
            var totalCanceledAppointments = await appointmentsQuery.CountAsync(a => a.AppointmentStatus == (short)AppointmentStatus.Canceled);

            var totalRevenue = await _context.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            var totalAdvicePosts = await _context.DoctorAdvicePosts.CountAsync();
            var totalAdviceVideos = await _context.DoctorAdviceVideos.CountAsync();
            var totalConversations = await _context.Conversations.CountAsync();
            var totalMedicines = await _context.Medicines.CountAsync();

            var newUsersToday = await _context.Users.CountAsync(u => u.DateOfRegistration.Date == today);
            var newUsersThisWeek = await _context.Users.CountAsync(u => u.DateOfRegistration.Date >= startOfWeek);
            var newUsersThisMonth = await _context.Users.CountAsync(u => u.DateOfRegistration.Date >= startOfMonth);

            var appointmentsToday = await appointmentsQuery.CountAsync(a => a.AppointmentDate.ToDateTime(TimeOnly.MinValue).Date == today);
            var appointmentsThisWeek = await appointmentsQuery.CountAsync(a => a.AppointmentDate.ToDateTime(TimeOnly.MinValue).Date >= startOfWeek);

            var revenueToday = await _context.Payments.Where(p => p.PaymentDate.Date == today).SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            var revenueThisWeek = await _context.Payments.Where(p => p.PaymentDate.Date >= startOfWeek).SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
            var revenueThisMonth = await _context.Payments.Where(p => p.PaymentDate.Date >= startOfMonth).SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

            var dto = new AdminSystemOverviewDto
            {
                TotalUsers = totalUsers,
                TotalDoctors = totalDoctors,
                TotalPatients = totalPatients,
                TotalAppointments = totalAppointments,
                TotalCompletedAppointments = totalCompletedAppointments,
                TotalPendingAppointments = totalPendingAppointments,
                TotalCanceledAppointments = totalCanceledAppointments,
                TotalRevenue = totalRevenue,
                TotalAdvicePosts = totalAdvicePosts,
                TotalAdviceVideos = totalAdviceVideos,
                TotalConversations = totalConversations,
                TotalMedicines = totalMedicines,
                NewUsersToday = newUsersToday,
                NewUsersThisWeek = newUsersThisWeek,
                NewUsersThisMonth = newUsersThisMonth,
                AppointmentsToday = appointmentsToday,
                AppointmentsThisWeek = appointmentsThisWeek,
                RevenueToday = revenueToday,
                RevenueThisWeek = revenueThisWeek,
                RevenueThisMonth = revenueThisMonth
            };

            return Result<AdminSystemOverviewDto>.Success(dto);
        }

        public async Task<Result<IEnumerable<AdminRegistrationTrendDto>>> GetRegistrationTrendsAsync(DateTime from, DateTime to)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.User.DateOfRegistration >= from && d.User.DateOfRegistration <= to)
                .GroupBy(d => d.User.DateOfRegistration.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.DateOfRegistration >= from && p.User.DateOfRegistration <= to)
                .GroupBy(p => p.User.DateOfRegistration.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var allDates = doctors.Select(d => d.Date).Union(patients.Select(p => p.Date)).Distinct().OrderBy(d => d).ToList();

            var trends = allDates.Select(date => new AdminRegistrationTrendDto
            {
                Date = date,
                DoctorCount = doctors.FirstOrDefault(d => d.Date == date)?.Count ?? 0,
                PatientCount = patients.FirstOrDefault(p => p.Date == date)?.Count ?? 0
            }).ToList();

            return Result<IEnumerable<AdminRegistrationTrendDto>>.Success(trends);
        }

        public async Task<Result<IEnumerable<AdminAppointmentTrendDto>>> GetAppointmentTrendsAsync(DateTime from, DateTime to)
        {
            var fromDate = DateOnly.FromDateTime(from);
            var toDate = DateOnly.FromDateTime(to);

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate)
                .GroupBy(a => a.AppointmentDate)
                .Select(g => new
                {
                    Date = g.Key,
                    Completed = g.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Completed),
                    Pending = g.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Pending),
                    Canceled = g.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Canceled),
                    Rescheduled = g.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Rescheduled)
                })
                .OrderBy(a => a.Date)
                .ToListAsync();

            var trends = appointments.Select(a => new AdminAppointmentTrendDto
            {
                Date = a.Date.ToDateTime(TimeOnly.MinValue),
                Completed = a.Completed,
                Pending = a.Pending,
                Canceled = a.Canceled,
                Rescheduled = a.Rescheduled
            }).ToList();

            return Result<IEnumerable<AdminAppointmentTrendDto>>.Success(trends);
        }

        public async Task<Result<IEnumerable<AdminRevenueTrendDto>>> GetRevenueTrendsAsync(DateTime from, DateTime to, string groupBy = "day")
        {
            var paymentsQuery = _context.Payments.Include(p => p.Appointment).Where(p => p.PaymentDate >= from && p.PaymentDate <= to);

            var trends = new List<AdminRevenueTrendDto>();

            if (groupBy.ToLower() == "month")
            {
                var grouped = await paymentsQuery
                    .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .Select(g => new AdminRevenueTrendDto
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Revenue = g.Sum(p => (decimal)p.AmountPaid),
                        AppointmentCount = g.Select(p => p.Appointment.AppointmentID).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
                trends = grouped;
            }
            else if (groupBy.ToLower() == "week")
            {
                var rawPayments = await paymentsQuery
                    .Select(p => new { p.PaymentDate, p.AmountPaid, p.Appointment.AppointmentID })
                    .ToListAsync();

                trends = rawPayments
                    .GroupBy(p => p.PaymentDate.Date.AddDays(-(int)p.PaymentDate.DayOfWeek))
                    .Select(g => new AdminRevenueTrendDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(p => (decimal)p.AmountPaid),
                        AppointmentCount = g.Select(p => p.AppointmentID).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else // day
            {
                var grouped = await paymentsQuery
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new AdminRevenueTrendDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(p => (decimal)p.AmountPaid),
                        AppointmentCount = g.Select(p => p.Appointment.AppointmentID).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
                trends = grouped;
            }

            return Result<IEnumerable<AdminRevenueTrendDto>>.Success(trends);
        }

        public async Task<Result<IEnumerable<AdminTopSpecializationDto>>> GetTopSpecializationsAsync(int count = 10)
        {
            var topSpecs = await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.Specialization))
                .GroupBy(d => d.Specialization)
                .Select(g => new AdminTopSpecializationDto
                {
                    Specialization = g.Key,
                    DoctorCount = g.Count(),
                    AppointmentCount = g.SelectMany(d => d.Appointments).Count()
                })
                .OrderByDescending(s => s.AppointmentCount)
                .ThenByDescending(s => s.DoctorCount)
                .Take(count)
                .ToListAsync();

            return Result<IEnumerable<AdminTopSpecializationDto>>.Success(topSpecs);
        }

        public async Task<Result<PaginatedResult<AdminUserListItemDto>>> GetAllUsersAsync(AdminUserFilterDto filter)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(u => u.FullName.Contains(filter.SearchTerm) || u.Email.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.Gender))
            {
                query = query.Where(u => u.Gender == filter.Gender);
            }

            if (filter.RegisteredFrom.HasValue)
            {
                query = query.Where(u => u.DateOfRegistration >= filter.RegisteredFrom.Value);
            }

            if (filter.RegisteredTo.HasValue)
            {
                query = query.Where(u => u.DateOfRegistration <= filter.RegisteredTo.Value);
            }
            
            if (filter.IsActive.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                if (filter.IsActive.Value)
                {
                    query = query.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= now);
                }
                else
                {
                    query = query.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > now);
                }
            }

            if (!string.IsNullOrEmpty(filter.Role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(filter.Role);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "fullname" => filter.SortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                "email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "dateofregistration" => filter.SortDescending ? query.OrderByDescending(u => u.DateOfRegistration) : query.OrderBy(u => u.DateOfRegistration),
                _ => filter.SortDescending ? query.OrderByDescending(u => u.DateOfRegistration) : query.OrderBy(u => u.DateOfRegistration)
            };

            var users = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .ToListAsync();

            var dtos = new List<AdminUserListItemDto>();
            var nowUtc = DateTimeOffset.UtcNow;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                dtos.Add(new AdminUserListItemDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    DateOfRegistration = user.DateOfRegistration,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Roles = roles.ToList(),
                    IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= nowUtc,
                    IsDoctor = user.Doctor != null,
                    IsPatient = user.Patient != null,
                    DoctorSpecialization = user.Doctor?.Specialization,
                    DoctorVerificationStatus = user.Doctor?.VerificationStatus.ToString()
                });
            }

            return Result<PaginatedResult<AdminUserListItemDto>>.Success(
                PaginatedResult<AdminUserListItemDto>.Create(dtos, totalCount, filter.Page, filter.PageSize)
            );
        }

        public async Task<Result<AdminUserDetailDto>> GetUserDetailAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Result<AdminUserDetailDto>.Failure("User not found.", ServiceErrorType.NotFound);

            var roles = await _userManager.GetRolesAsync(user);
            var nowUtc = DateTimeOffset.UtcNow;

            var dto = new AdminUserDetailDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                DateOfRegistration = user.DateOfRegistration,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = roles.ToList(),
                IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= nowUtc,
                IsDoctor = user.Doctor != null,
                IsPatient = user.Patient != null,
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed
            };

            if (user.Doctor != null)
            {
                var doctorAppts = await _context.Appointments.Where(a => a.DoctorID == userId).ToListAsync();
                dto.DoctorSpecialization = user.Doctor.Specialization;
                dto.DoctorVerificationStatus = user.Doctor.VerificationStatus.ToString();
                dto.DoctorPrice = user.Doctor.Price;
                dto.DoctorRating = user.Doctor.Rating;
                dto.DoctorRatingCount = user.Doctor.RatingCount;
                dto.DoctorIsAvailable = user.Doctor.IsAvailable;
                dto.DoctorIdNo = user.Doctor.IdNo;
                dto.DoctorIdImageFrontUrl = user.Doctor.IdImageFrontUrl;
                dto.DoctorIdImageBackUrl = user.Doctor.IdImageBackUrl;
                
                dto.TotalAppointments = doctorAppts.Count;
                dto.CompletedAppointments = doctorAppts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Completed);
                dto.CanceledAppointments = doctorAppts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Canceled);
                
                dto.TotalEarnings = await _context.DoctorEarnings.Where(e => e.DoctorId == userId).SumAsync(e => e.DailyEarnings);
                dto.TotalAdvicePosts = await _context.DoctorAdvicePosts.CountAsync(p => p.DoctorId == userId);
                dto.TotalAdviceVideos = await _context.DoctorAdviceVideos.CountAsync(v => v.DoctorId == userId);
            }
            else if (user.Patient != null)
            {
                var patientAppts = await _context.Appointments.Where(a => a.PatientID == userId).ToListAsync();
                dto.TotalAppointments = patientAppts.Count;
                dto.CompletedAppointments = patientAppts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Completed);
                dto.CanceledAppointments = patientAppts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Canceled);
                
                var medicalRecord = await _context.MedicalRecords.Include(mr => mr.Prescriptions).FirstOrDefaultAsync(mr => mr.PatientId == userId);
                dto.MedicalRecordId = medicalRecord?.MedicalRecordId;
                dto.TotalPrescriptions = medicalRecord?.Prescriptions.Count ?? 0;
            }

            return Result<AdminUserDetailDto>.Success(dto);
        }

        public async Task<Result<bool>> BanUserAsync(string userId, string? reason)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found.", ServiceErrorType.NotFound);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SuperAdmin))
                return Result<bool>.Failure("Cannot ban a SuperAdmin.", ServiceErrorType.Conflict);

            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // effectively permanently banned
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return Result<bool>.Failure("Failed to ban user.", ServiceErrorType.ServerError);


            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found.", ServiceErrorType.NotFound);

            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return Result<bool>.Failure("Failed to activate user.", ServiceErrorType.ServerError);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ChangeUserRoleAsync(string userId, AdminChangeUserRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found.", ServiceErrorType.NotFound);

            if (dto.Action.Equals("Add", StringComparison.OrdinalIgnoreCase))
            {
                if (!await _userManager.IsInRoleAsync(user, dto.RoleName))
                {
                    var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
                    if (!result.Succeeded)
                        return Result<bool>.Failure($"Failed to add role. {string.Join(", ", result.Errors.Select(e => e.Description))}", ServiceErrorType.ValidationError);
                }
            }
            else if (dto.Action.Equals("Remove", StringComparison.OrdinalIgnoreCase))
            {
                if (dto.RoleName == Roles.SuperAdmin)
                {
                    var superAdmins = await _userManager.GetUsersInRoleAsync(Roles.SuperAdmin);
                    if (superAdmins.Count <= 1 && superAdmins.Any(u => u.Id == userId))
                        return Result<bool>.Failure("Cannot remove the last SuperAdmin role.", ServiceErrorType.Conflict);
                }

                if (await _userManager.IsInRoleAsync(user, dto.RoleName))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
                    if (!result.Succeeded)
                        return Result<bool>.Failure($"Failed to remove role. {string.Join(", ", result.Errors.Select(e => e.Description))}", ServiceErrorType.ValidationError);
                }
            }
            else
            {
                return Result<bool>.Failure("Invalid action. Must be 'Add' or 'Remove'.", ServiceErrorType.ValidationError);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found.", ServiceErrorType.NotFound);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SuperAdmin))
                return Result<bool>.Failure("Cannot delete a SuperAdmin.", ServiceErrorType.Conflict);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return Result<bool>.Failure($"Failed to delete user. {string.Join(", ", result.Errors.Select(e => e.Description))}", ServiceErrorType.ServerError);

            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResult<AdminDoctorListItemDto>>> GetAllDoctorsAsync(AdminDoctorFilterDto filter)
        {
            var query = _context.Doctors.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(d => d.User.FullName.Contains(filter.SearchTerm) || d.User.Email.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.Specialization))
            {
                query = query.Where(d => d.Specialization == filter.Specialization);
            }

            if (!string.IsNullOrEmpty(filter.VerificationStatus))
            {
                if (Enum.TryParse<DoctorVerificationStatus>(filter.VerificationStatus, true, out var status))
                {
                    query = query.Where(d => d.VerificationStatus == status);
                }
            }

            if (filter.IsAvailable.HasValue)
            {
                query = query.Where(d => d.IsAvailable == filter.IsAvailable.Value);
            }

            if (filter.MinRating.HasValue)
            {
                query = query.Where(d => d.Rating >= filter.MinRating.Value);
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "fullname" => filter.SortDescending ? query.OrderByDescending(d => d.User.FullName) : query.OrderBy(d => d.User.FullName),
                "rating" => filter.SortDescending ? query.OrderByDescending(d => d.Rating) : query.OrderBy(d => d.Rating),
                "dateofregistration" => filter.SortDescending ? query.OrderByDescending(d => d.User.DateOfRegistration) : query.OrderBy(d => d.User.DateOfRegistration),
                _ => filter.SortDescending ? query.OrderByDescending(d => d.User.DateOfRegistration) : query.OrderBy(d => d.User.DateOfRegistration)
            };

            var doctors = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = new List<AdminDoctorListItemDto>();
            foreach (var doc in doctors)
            {
                var docAppts = await _context.Appointments.Where(a => a.DoctorID == doc.Id).ToListAsync();
                var earnings = await _context.DoctorEarnings.Where(e => e.DoctorId == doc.Id).SumAsync(e => e.DailyEarnings);
                var posts = await _context.DoctorAdvicePosts.CountAsync(p => p.DoctorId == doc.Id);
                var videos = await _context.DoctorAdviceVideos.CountAsync(v => v.DoctorId == doc.Id);

                dtos.Add(new AdminDoctorListItemDto
                {
                    Id = doc.Id,
                    FullName = doc.User.FullName,
                    Email = doc.User.Email ?? string.Empty,
                    Specialization = doc.Specialization,
                    Price = doc.Price,
                    Rating = doc.Rating,
                    RatingCount = doc.RatingCount,
                    IsAvailable = doc.IsAvailable,
                    VerificationStatus = doc.VerificationStatus.ToString(),
                    TotalAppointments = docAppts.Count,
                    CompletedAppointments = docAppts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Completed),
                    TotalEarnings = earnings,
                    TotalAdvicePosts = posts,
                    TotalAdviceVideos = videos,
                    ProfileImageUrl = doc.User.ProfileImageUrl,
                    DateOfRegistration = doc.User.DateOfRegistration
                });
            }

            return Result<PaginatedResult<AdminDoctorListItemDto>>.Success(
                PaginatedResult<AdminDoctorListItemDto>.Create(dtos, totalCount, filter.Page, filter.PageSize)
            );
        }

        public async Task<Result<bool>> VerifyDoctorAsync(string doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
                return Result<bool>.Failure("Doctor not found.", ServiceErrorType.NotFound);

            doctor.VerificationStatus = DoctorVerificationStatus.Verified;
            doctor.RejectionReason = null;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectDoctorAsync(string doctorId, string reason)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
                return Result<bool>.Failure("Doctor not found.", ServiceErrorType.NotFound);

            if (string.IsNullOrWhiteSpace(reason))
                return Result<bool>.Failure("Rejection reason is required.", ServiceErrorType.ValidationError);

            doctor.VerificationStatus = DoctorVerificationStatus.Rejected;
            doctor.RejectionReason = reason;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ForceDoctorPriceAsync(string doctorId, decimal price)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
                return Result<bool>.Failure("Doctor not found.", ServiceErrorType.NotFound);

            if (price < 0)
                return Result<bool>.Failure("Price cannot be negative.", ServiceErrorType.ValidationError);

            doctor.Price = price;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResult<AdminPatientListItemDto>>> GetAllPatientsAsync(AdminUserFilterDto filter)
        {
            var query = _context.Patients.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => p.User.FullName.Contains(filter.SearchTerm) || p.User.Email.Contains(filter.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var patients = await query
                .OrderByDescending(p => p.User.DateOfRegistration)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = new List<AdminPatientListItemDto>();
            var nowUtc = DateTimeOffset.UtcNow;

            foreach (var p in patients)
            {
                var appts = await _context.Appointments.Where(a => a.PatientID == p.Id).ToListAsync();
                var mrCount = await _context.MedicalRecords.CountAsync(mr => mr.PatientId == p.Id);

                dtos.Add(new AdminPatientListItemDto
                {
                    Id = p.Id,
                    FullName = p.User.FullName,
                    Email = p.User.Email ?? string.Empty,
                    Gender = p.User.Gender,
                    DateOfBirth = p.User.DateOfBirth,
                    DateOfRegistration = p.User.DateOfRegistration,
                    ProfileImageUrl = p.User.ProfileImageUrl,
                    TotalAppointments = appts.Count,
                    CompletedAppointments = appts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Completed),
                    CanceledAppointments = appts.Count(a => a.AppointmentStatus == (short)AppointmentStatus.Canceled),
                    HasMedicalRecord = mrCount > 0,
                    IsActive = !p.User.LockoutEnd.HasValue || p.User.LockoutEnd.Value <= nowUtc
                });
            }

            return Result<PaginatedResult<AdminPatientListItemDto>>.Success(
                PaginatedResult<AdminPatientListItemDto>.Create(dtos, totalCount, filter.Page, filter.PageSize)
            );
        }

        public async Task<Result<PaginatedResult<AdminAppointmentListItemDto>>> GetAllAppointmentsAsync(AdminAppointmentFilterDto filter)
        {
            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Payment)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(a => a.Patient.User.FullName.Contains(filter.SearchTerm) || a.Doctor.User.FullName.Contains(filter.SearchTerm));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.AppointmentStatus == filter.Status.Value);
            }

            if (!string.IsNullOrEmpty(filter.DoctorId))
            {
                query = query.Where(a => a.DoctorID == filter.DoctorId);
            }

            if (!string.IsNullOrEmpty(filter.PatientId))
            {
                query = query.Where(a => a.PatientID == filter.PatientId);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= filter.DateTo.Value);
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "date" => filter.SortDescending ? query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime) : query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime),
                _ => filter.SortDescending ? query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime) : query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
            };

            var appointments = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = appointments.Select(a => new AdminAppointmentListItemDto
            {
                AppointmentId = a.AppointmentID,
                PatientId = a.PatientID,
                PatientName = a.Patient.User.FullName,
                DoctorId = a.DoctorID,
                DoctorName = a.Doctor.User.FullName,
                DoctorSpecialization = a.Doctor.Specialization,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = ((AppointmentStatus)a.AppointmentStatus).ToString(),
                HasMedicalRecord = a.MedicalRecordId.HasValue,
                HasPayment = a.PaymentID.HasValue,
                PaymentAmount = a.Payment?.AmountPaid,
                AdditionalNotes = a.AdditionalNotes
            }).ToList();

            return Result<PaginatedResult<AdminAppointmentListItemDto>>.Success(
                PaginatedResult<AdminAppointmentListItemDto>.Create(dtos, totalCount, filter.Page, filter.PageSize)
            );
        }

        public async Task<Result<bool>> ForceCancelAppointmentAsync(Guid appointmentId, string? reason)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
            if (appointment == null)
                return Result<bool>.Failure("Appointment not found.", ServiceErrorType.NotFound);

            appointment.AppointmentStatus = (short)AppointmentStatus.Canceled;
            if (!string.IsNullOrEmpty(reason))
            {
                appointment.AdditionalNotes = string.IsNullOrEmpty(appointment.AdditionalNotes) 
                    ? $"Admin Canceled: {reason}" 
                    : $"{appointment.AdditionalNotes} | Admin Canceled: {reason}";
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ForceCompleteAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
            if (appointment == null)
                return Result<bool>.Failure("Appointment not found.", ServiceErrorType.NotFound);

            appointment.AppointmentStatus = (short)AppointmentStatus.Completed;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResult<AdminAdvicePostListDto>>> GetAllAdvicePostsAsync(int page, int pageSize)
        {
            var query = _context.DoctorAdvicePosts
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AdminAdvicePostListDto
                {
                    Id = p.Id,
                    DoctorId = p.DoctorId,
                    DoctorName = p.Doctor.User.FullName,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    IsPublished = p.IsPublished,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count
                })
                .ToListAsync();

            return Result<PaginatedResult<AdminAdvicePostListDto>>.Success(
                PaginatedResult<AdminAdvicePostListDto>.Create(posts, totalCount, page, pageSize)
            );
        }

        public async Task<Result<bool>> DeleteAdvicePostAsync(Guid postId)
        {
            var post = await _context.DoctorAdvicePosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return Result<bool>.Failure("Post not found.", ServiceErrorType.NotFound);

            _context.DoctorAdvicePosts.Remove(post);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> TogglePostPublishAsync(Guid postId)
        {
            var post = await _context.DoctorAdvicePosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return Result<bool>.Failure("Post not found.", ServiceErrorType.NotFound);

            post.IsPublished = !post.IsPublished;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResult<AdminAdviceVideoListDto>>> GetAllAdviceVideosAsync(int page, int pageSize)
        {
            var query = _context.DoctorAdviceVideos
                .Include(v => v.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var videos = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new AdminAdviceVideoListDto
                {
                    Id = v.Id,
                    DoctorId = v.DoctorId,
                    DoctorName = v.Doctor.User.FullName,
                    Title = v.Title,
                    Description = v.Description,
                    VideoUrl = v.VideoUrl,
                    CreatedAt = v.CreatedAt,
                    IsPublished = v.IsPublished
                })
                .ToListAsync();

            return Result<PaginatedResult<AdminAdviceVideoListDto>>.Success(
                PaginatedResult<AdminAdviceVideoListDto>.Create(videos, totalCount, page, pageSize)
            );
        }

        public async Task<Result<bool>> DeleteAdviceVideoAsync(Guid videoId)
        {
            var video = await _context.DoctorAdviceVideos.FirstOrDefaultAsync(v => v.Id == videoId);
            if (video == null)
                return Result<bool>.Failure("Video not found.", ServiceErrorType.NotFound);

            _context.DoctorAdviceVideos.Remove(video);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ToggleVideoPublishAsync(Guid videoId)
        {
            var video = await _context.DoctorAdviceVideos.FirstOrDefaultAsync(v => v.Id == videoId);
            if (video == null)
                return Result<bool>.Failure("Video not found.", ServiceErrorType.NotFound);

            video.IsPublished = !video.IsPublished;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<AdminFinancialSummaryDto>> GetFinancialSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            var allPayments = await _context.Payments.ToListAsync();
            
            var totalRevenue = allPayments.Sum(p => (decimal)p.AmountPaid);
            var revenueToday = allPayments.Where(p => p.PaymentDate.Date == today).Sum(p => (decimal)p.AmountPaid);
            var revenueThisWeek = allPayments.Where(p => p.PaymentDate.Date >= startOfWeek).Sum(p => (decimal)p.AmountPaid);
            var revenueThisMonth = allPayments.Where(p => p.PaymentDate.Date >= startOfMonth).Sum(p => (decimal)p.AmountPaid);
            var revenueThisYear = allPayments.Where(p => p.PaymentDate.Date >= startOfYear).Sum(p => (decimal)p.AmountPaid);

            var firstPaymentDate = allPayments.OrderBy(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;
            decimal avgPerDay = 0;
            if (firstPaymentDate.HasValue)
            {
                var days = (today - firstPaymentDate.Value.Date).TotalDays;
                if (days > 0) avgPerDay = totalRevenue / (decimal)days;
                else avgPerDay = totalRevenue; // Only 1 day
            }

            var topDoctorResult = await GetRevenueByDoctorAsync(1);
            var topDoctor = topDoctorResult.Data?.FirstOrDefault();
            
            AdminTopEarnerDto? topEarner = null;
            if (topDoctor != null)
            {
                topEarner = new AdminTopEarnerDto
                {
                    DoctorId = topDoctor.DoctorId,
                    DoctorName = topDoctor.DoctorName,
                    Specialization = topDoctor.Specialization,
                    TotalEarnings = topDoctor.TotalEarnings
                };
            }

            var dto = new AdminFinancialSummaryDto
            {
                TotalRevenue = totalRevenue,
                RevenueToday = revenueToday,
                RevenueThisWeek = revenueThisWeek,
                RevenueThisMonth = revenueThisMonth,
                RevenueThisYear = revenueThisYear,
                AverageRevenuePerDay = avgPerDay,
                TotalPayments = allPayments.Count,
                TopEarningDoctor = topEarner
            };

            return Result<AdminFinancialSummaryDto>.Success(dto);
        }

        public async Task<Result<IEnumerable<AdminDoctorRevenueDto>>> GetRevenueByDoctorAsync(int count = 20)
        {
            var topEarners = await _context.DoctorEarnings
                .Include(e => e.Doctor).ThenInclude(d => d.User)
                .GroupBy(e => new { e.DoctorId, e.Doctor.User.FullName, e.Doctor.Specialization })
                .Select(g => new AdminDoctorRevenueDto
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.FullName,
                    Specialization = g.Key.Specialization,
                    TotalEarnings = g.Sum(e => e.DailyEarnings),
                    CompletedAppointments = g.Sum(e => e.AppointmentCount),
                    AveragePerAppointment = g.Sum(e => e.AppointmentCount) > 0 ? g.Sum(e => e.DailyEarnings) / g.Sum(e => e.AppointmentCount) : 0
                })
                .OrderByDescending(r => r.TotalEarnings)
                .Take(count)
                .ToListAsync();

            return Result<IEnumerable<AdminDoctorRevenueDto>>.Success(topEarners);
        }

        public async Task<Result<IEnumerable<AdminRecentActivityDto>>> GetRecentActivityAsync(int count = 50)
        {
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.DateOfRegistration)
                .Take(count)
                .Select(u => new AdminRecentActivityDto
                {
                    Type = "NewUser",
                    Description = $"New user registered: {u.Email}",
                    UserId = u.Id,
                    UserName = u.FullName,
                    Timestamp = u.DateOfRegistration,
                    EntityId = u.Id
                })
                .ToListAsync();

            var recentAppts = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime)
                .Take(count)
                .Select(a => new AdminRecentActivityDto
                {
                    Type = "Appointment",
                    Description = $"Appointment status: {((AppointmentStatus)a.AppointmentStatus)}",
                    UserId = a.PatientID,
                    UserName = a.Patient.User.FullName,
                    Timestamp = a.AppointmentDate.ToDateTime(TimeOnly.MinValue), // Approximated
                    EntityId = a.AppointmentID.ToString()
                })
                .ToListAsync();

            var recentPosts = await _context.DoctorAdvicePosts
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Select(p => new AdminRecentActivityDto
                {
                    Type = "PostCreated",
                    Description = $"New advice post by {p.Doctor.User.FullName}",
                    UserId = p.DoctorId,
                    UserName = p.Doctor.User.FullName,
                    Timestamp = p.CreatedAt,
                    EntityId = p.Id.ToString()
                })
                .ToListAsync();

            var combined = recentUsers.Concat(recentAppts).Concat(recentPosts)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();

            return Result<IEnumerable<AdminRecentActivityDto>>.Success(combined);
        }

        public async Task<Result<PaginatedResult<AdminDoctorRatingListDto>>> GetAllRatingsAsync(int page, int pageSize)
        {
            var query = _context.DoctorRatings
                .Include(r => r.Doctor).ThenInclude(d => d.User)
                .Include(r => r.Patient).ThenInclude(p => p.User)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var ratings = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminDoctorRatingListDto
                {
                    Id = r.Id,
                    DoctorId = r.DoctorID,
                    DoctorName = r.Doctor.User.FullName,
                    PatientId = r.PatientID,
                    PatientName = r.Patient.User.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    AppointmentId = r.AppointmentID
                })
                .ToListAsync();

            return Result<PaginatedResult<AdminDoctorRatingListDto>>.Success(
                PaginatedResult<AdminDoctorRatingListDto>.Create(ratings, totalCount, page, pageSize)
            );
        }

        public async Task<Result<bool>> DeleteRatingAsync(int ratingId)
        {
            var rating = await _context.DoctorRatings.FirstOrDefaultAsync(r => r.Id == ratingId);
            if (rating == null)
                return Result<bool>.Failure("Rating not found.", ServiceErrorType.NotFound);

            _context.DoctorRatings.Remove(rating);
            await _context.SaveChangesAsync();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == rating.DoctorID);
            if (doctor != null)
            {
                var remainingRatings = await _context.DoctorRatings.Where(r => r.DoctorID == doctor.Id).ToListAsync();
                if (remainingRatings.Any())
                {
                    doctor.Rating = (decimal)remainingRatings.Average(r => r.Rating);
                    doctor.RatingCount = remainingRatings.Count;
                }
                else
                {
                    doctor.Rating = null;
                    doctor.RatingCount = 0;
                }
                await _context.SaveChangesAsync();
            }

            return Result<bool>.Success(true);
        }
    }
}
