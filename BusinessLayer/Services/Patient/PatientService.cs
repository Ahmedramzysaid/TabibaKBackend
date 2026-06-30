using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using BusinessLayer.Validations;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.ServicesInterfaces;
using DomainLayer.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Persistence;

namespace BusinessLayer.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly DomainLayer.Interfaces.IEmailService _emailService;

        public PatientService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, DomainLayer.Interfaces.IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
        }

        public async Task<Result<PatientDto>> Add(PatientDto patientDto, string password)
        {
            var validations = ValidatePatient(patientDto, GeneralEnum.SaveMode.Add);
            if (!validations.IsValid)
                return Result<PatientDto>.Failure(validations.ErrorMessage, ServiceErrorType.ValidationError);

            if (!string.IsNullOrEmpty(patientDto.Id))
            {
                var existingPatient = await _unitOfWork.Patients.GetById(patientDto.Id);
                if (existingPatient != null)
                {
                    return Result<PatientDto>.Failure("Patient with this ID already exists. Please leave ID empty for new patients.", ServiceErrorType.ValidationError);
                }
            }

            if (string.IsNullOrWhiteSpace(patientDto.Email))
            {
                return Result<PatientDto>.Failure("Email is required", ServiceErrorType.ValidationError);
            }

            if (!string.IsNullOrWhiteSpace(patientDto.Email))
            {
                var existingEmailUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == patientDto.Email.Trim());
                if (existingEmailUser != null)
                {
                    return Result<PatientDto>.Failure("User with this email already exists. Please use a different email.", ServiceErrorType.ValidationError);
                }
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return Result<PatientDto>.Failure("Password is required", ServiceErrorType.ValidationError);
            }

            var userName = patientDto.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            var applicationUser = new ApplicationUser
            {
                FullName = patientDto.FullName,
                Email = patientDto.Email,
                UserName = userName,
                EmailConfirmed = true,
                DateOfBirth = patientDto.DateOfBirth ?? DateTime.UtcNow,
                Gender = patientDto.Gender,
                Latitude = patientDto.Latitude ?? 0,
                Longitude = patientDto.Longitude ?? 0,
                DateOfRegistration = patientDto.DateOfRegistration != default ? patientDto.DateOfRegistration : DateTime.UtcNow,
                ProfileImageUrl = patientDto.ProfileImageUrl
            };

            var createUserResult = await _userManager.CreateAsync(applicationUser, password);

            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                return Result<PatientDto>.Failure($"Failed to create user: {errors}", ServiceErrorType.ValidationError);
            }

            try
            {
                var patientRole = await _roleManager.FindByNameAsync(Roles.Patient);
                if (patientRole == null)
                {
                    patientRole = new IdentityRole(Roles.Patient)
                    {
                        NormalizedName = Roles.Patient.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };
                    var createRoleResult = await _roleManager.CreateAsync(patientRole);
                    if (!createRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
                    }
                }

                if (patientRole != null)
                {
                    var existingRoles = await _userManager.GetRolesAsync(applicationUser);
                    if (!existingRoles.Contains(Roles.Patient))
                    {
                        var addToRoleResult = await _userManager.AddToRoleAsync(applicationUser, Roles.Patient);
                        if (!addToRoleResult.Succeeded)
                        {
                            var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            var patient = new Patient
            {
                Id = applicationUser.Id, // Same ID as ApplicationUser (auto-generated)
            };

            await _unitOfWork.Patients.Add(patient);
            var result = await _unitOfWork.SaveChanges();

            if (!result)
            {
                await _userManager.DeleteAsync(applicationUser);
                return Result<PatientDto>.Failure("Failed to add new patient in database", ServiceErrorType.DatabaseError);
            }

            var medicalRecord = new MedicalRecord
            {
                MedicalRecordId = Guid.NewGuid(),
                PatientId = patient.Id
            };

            await _unitOfWork.MedicalRecords.Add(medicalRecord);
            var medicalRecordResult = await _unitOfWork.SaveChanges();

            if (!medicalRecordResult)
            {
                _unitOfWork.Patients.Delete(patient);
                await _unitOfWork.SaveChanges();
                await _userManager.DeleteAsync(applicationUser);
                return Result<PatientDto>.Failure("Failed to create medical record for patient", ServiceErrorType.DatabaseError);
            }

            try
            {
                var prescriptionClaims = new[]
                {
                    new Claim(ClaimConstants.Permission, ClaimConstants.ViewPrescriptions),
                    new Claim(ClaimConstants.Permission, ClaimConstants.CreatePrescription),
                    new Claim(ClaimConstants.Permission, ClaimConstants.EditPrescription),
                    new Claim(ClaimConstants.Permission, ClaimConstants.DeletePrescription)
                };

                var testingClaims = new[]
                {
                    new Claim(ClaimConstants.Permission, ClaimConstants.ViewMedicalRecords),
                    new Claim(ClaimConstants.Permission, ClaimConstants.CreateMedicalRecord),
                    new Claim(ClaimConstants.Permission, ClaimConstants.EditMedicalRecord),
                    new Claim(ClaimConstants.Permission, ClaimConstants.DeleteMedicalRecord)
                };

                var existingClaims = await _userManager.GetClaimsAsync(applicationUser);
                
                var allClaims = prescriptionClaims.Concat(testingClaims).ToList();
                foreach (var claim in allClaims)
                {
                    var claimExists = existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value);
                    
                    if (!claimExists)
                    {
                        var addClaimResult = await _userManager.AddClaimAsync(applicationUser, claim);
                        if (!addClaimResult.Succeeded)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (!string.IsNullOrWhiteSpace(applicationUser.Email))
            {
                var body = EmailTemplates.WelcomePatient(applicationUser.FullName ?? applicationUser.UserName);
                _ = _emailService.SendEmailAsync(applicationUser.Email.Trim(), "Welcome to Tabibak", body, isHtml: true);
            }

            var createdPatientDto = _mapper.Map<PatientDto>(patient);
            createdPatientDto.MedicalRecordId = medicalRecord.MedicalRecordId; // Include MedicalRecordId in response
            return Result<PatientDto>.Success(createdPatientDto);
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        private ValidationsResult ValidatePatient(PatientDto patient, GeneralEnum.SaveMode saveMode)
        {
            var validator = new PatientValidator(saveMode);
            var validationResult = validator.Validate(patient);
            if (!validationResult.IsValid)
            {
                string message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ValidationsResult(false, message);
            }
            return new ValidationsResult(true, "");
        }

        public async Task<Result<PatientDto>> Update(PatientDto patient)
        {
            if (patient == null)
                return Result<PatientDto>.Failure("Patient can not be null", ServiceErrorType.ValidationError);

            var validations = ValidatePatient(patient, GeneralEnum.SaveMode.Update);
            if (!validations.IsValid)
                return Result<PatientDto>.Failure(validations.ErrorMessage, ServiceErrorType.ValidationError);

            var updatedPatient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patient.Id);

            if (updatedPatient == null)
                return Result<PatientDto>.Failure("Patient is not found to update", ServiceErrorType.NotFound);

            if (updatedPatient.User != null)
            {
                if (patient.FullName != updatedPatient.User.FullName)
                {
                    updatedPatient.User.FullName = patient.FullName;
                }

                if (patient.DateOfBirth.HasValue && patient.DateOfBirth != updatedPatient.User.DateOfBirth)
                {
                    updatedPatient.User.DateOfBirth = patient.DateOfBirth.Value;
                }

                if (patient.Gender != updatedPatient.User.Gender)
                {
                    updatedPatient.User.Gender = patient.Gender;
                }

                if (patient.Latitude.HasValue && patient.Latitude != updatedPatient.User.Latitude)
                {
                    updatedPatient.User.Latitude = patient.Latitude.Value;
                }

                if (patient.Longitude.HasValue && patient.Longitude != updatedPatient.User.Longitude)
                {
                    updatedPatient.User.Longitude = patient.Longitude.Value;
                }

                if (patient.ProfileImageUrl != updatedPatient.User.ProfileImageUrl)
                {
                    updatedPatient.User.ProfileImageUrl = patient.ProfileImageUrl;
                }
                
                if (string.IsNullOrWhiteSpace(patient.Email))
                {
                    return Result<PatientDto>.Failure("Email is required.", ServiceErrorType.ValidationError);
                }
                if (patient.Email.Trim() != updatedPatient.User.Email)
                {
                    var existingByEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == patient.Email.Trim());
                    if (existingByEmail != null && existingByEmail.Id != updatedPatient.Id)
                    {
                        return Result<PatientDto>.Failure("User with this email already exists. Please use a different email.", ServiceErrorType.ValidationError);
                    }
                    var emailChangeResult = await _userManager.SetEmailAsync(updatedPatient.User, patient.Email.Trim());
                    if (!emailChangeResult.Succeeded)
                    {
                        return Result<PatientDto>.Failure($"Failed to update email: {string.Join(", ", emailChangeResult.Errors.Select(e => e.Description))}", ServiceErrorType.ValidationError);
                    }
                }
            }

            _unitOfWork.Patients.Update(updatedPatient);

            var result = await _unitOfWork.SaveChanges();

            if (!result)
            {
                return Result<PatientDto>.Failure("Failed to update the patient", ServiceErrorType.DatabaseError);
            }

            return Result<PatientDto>.Success();
        }

        public async Task<Result<PaginatedResult<PatientDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Patients
                .Include(p => p.User)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
                return Result<PaginatedResult<PatientDto>>.Failure("No patients found", ServiceErrorType.NotFound);

            var patients = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = _mapper.Map<IEnumerable<PatientDto>>(patients);
            var paged = PaginatedResult<PatientDto>.Create(result, totalCount, pageNumber, pageSize);
            return Result<PaginatedResult<PatientDto>>.Success(paged);
        }

        public async Task<Result<PatientDto>> GetById(string id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient is null)
            {
                return Result<PatientDto>
                    .Failure("Invalid patient id, the patient with this id is not found.",
                    ServiceErrorType.NotFound);
            }

            var patientDto = _mapper.Map<PatientDto>(patient);
            return Result<PatientDto>.Success(patientDto);
        }

        public async Task<Result<PatientDto>> Delete(string patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient is null)
                return Result<PatientDto>
                    .Failure($"Invalid patient ID, there was not patient with id  = {patientId}",
                    ServiceErrorType.NotFound);

            var user = patient.User;
            if (user == null)
            {
                _unitOfWork.Patients.Delete(patient);
                var deleted = await _unitOfWork.SaveChanges();
                return deleted ?
                    Result<PatientDto>.Success()
                    : Result<PatientDto>.Failure("Some error occurred during deleting patient in database.", ServiceErrorType.DatabaseError);
            }

            _unitOfWork.Patients.Delete(patient);
            var patientDeleted = await _unitOfWork.SaveChanges();

            if (!patientDeleted)
            {
                return Result<PatientDto>.Failure("Some error occurred during deleting patient in database.", ServiceErrorType.DatabaseError);
            }

            var deleteUserResult = await _userManager.DeleteAsync(user);

            if (!deleteUserResult.Succeeded)
            {
                var errors = string.Join(", ", deleteUserResult.Errors.Select(e => e.Description));
                return Result<PatientDto>.Failure($"Patient record deleted but failed to delete user: {errors}", ServiceErrorType.DatabaseError);
            }

            return Result<PatientDto>.Success();
        }

        public async Task<Result<Patient>> Delete(Expression<Func<Patient, bool>> predicate)
        {
            try
            {
                var result = await _unitOfWork.Patients.Delete(predicate);
                return Result<Patient>.Success();
            }
            catch (Exception ex)
            {
                return Result<Patient>.Failure("Some error occurred during deleting in database.", ServiceErrorType.ServerError);
            }
        }
    }
}
