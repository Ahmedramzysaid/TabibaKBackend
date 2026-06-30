using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BusinessLayer.Validations;
using ClinicAPI.Helpers;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtOptions _jwt;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;

        private const string ForgotPasswordCacheKeyPrefix = "forgot_pwd:";
        private static readonly TimeSpan ForgotPasswordCodeExpiry = TimeSpan.FromMinutes(5);
        private const string ChangePasswordCacheKeyPrefix = "change_pwd:";
        private static readonly TimeSpan ChangePasswordCodeExpiry = TimeSpan.FromMinutes(5);
        private const string EmailVerifyCacheKeyPrefix = "email_verify:";
        private const string PendingRegCacheKeyPrefix = "pending_reg:";
        private static readonly TimeSpan EmailVerifyCodeExpiry = TimeSpan.FromSeconds(60);

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IOptions<JwtOptions> jwt, IMapper mapper, IUnitOfWork unitOfWork, IEmailService emailService,
            IMemoryCache cache, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _jwt = jwt.Value;
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Email))
            {
                return new AuthResponseDto
                {
                    Message = "Email is required",
                    IsAuthenticated = false
                };
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email.Trim());

            if (user is null)
            {
                return new AuthResponseDto
                {
                    Message = "Email not found in the system",
                    IsAuthenticated = false
                };
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    Message = "Password is incorrect",
                    IsAuthenticated = false
                };
            }

            var rolesBeforeFix = await _userManager.GetRolesAsync(user);
            if (rolesBeforeFix.Count == 0)
            {
                var hasPatient = await _unitOfWork.Patients.Find(p => p.Id == user.Id) != null;
                var hasDoctor = await _unitOfWork.Doctors.Find(d => d.Id == user.Id) != null;
                if (hasPatient)
                {
                    var patientRole = await _roleManager.FindByNameAsync(Roles.Patient);
                    if (patientRole != null)
                        await _userManager.AddToRoleAsync(user, Roles.Patient);
                }
                else if (hasDoctor)
                {
                    var doctorRole = await _roleManager.FindByNameAsync(Roles.Doctor);
                    if (doctorRole != null)
                        await _userManager.AddToRoleAsync(user, Roles.Doctor);
                }
            }

            var token = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var refreshToken = await GetActiveRefreshToken(user);
            
            Guid? medicalRecordId = null;
            string? doctorId = null;
            string? patientId = null;

            if (roles.Any(r => r.Equals("Doctor", StringComparison.OrdinalIgnoreCase)))
            {
                var doctor = await _unitOfWork.Doctors.Find(d => d.Id == user.Id);
                doctorId = doctor?.Id;
            }

            if (roles.Any(r => r.Equals("Patient", StringComparison.OrdinalIgnoreCase)))
            {
                var medicalRecord = await _unitOfWork.MedicalRecords.Find(m => m.PatientId == user.Id);
                medicalRecordId = medicalRecord?.MedicalRecordId;
                patientId = user.Id;
            }
            
            return new AuthResponseDto
            {
                Message = "Login successful",
                Token = token,
                IsAuthenticated = true,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresOn = refreshToken.ExpiresOn,
                MedicalRecordId = medicalRecordId,
                DoctorId = doctorId,
                PatientId = patientId
            };
        }

        public async Task<AuthResponseDto> RefreshToken(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user is null)
                return new AuthResponseDto
                {
                    Message = "Invalid token",
                    IsAuthenticated = false
                };

            var refreshToken = await _unitOfWork.RefreshTokens.Find(t => t.Token == token);
            if (refreshToken == null || !refreshToken.IsActive)
                return new AuthResponseDto
                {
                    Message = "Invalid token",
                    IsAuthenticated = false
                };

            refreshToken.RevokedOn = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.SaveChanges();

            var newRefreshToken = GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokens.Add(newRefreshToken);
            await _unitOfWork.SaveChanges();
            var newJwtToken = await CreateJwtToken(user);
            return new AuthResponseDto
            {
                Message = "Token refreshed successfully",
                Token = newJwtToken,
                IsAuthenticated = true,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiresOn = newRefreshToken.ExpiresOn,
                Email = user.Email,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };
        }

        public async Task<bool> RevokeToken(string token)
        {
            var refreshToken = await _unitOfWork.RefreshTokens.Find(t => t.Token == token);
            
            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            var validator = new RegisterValidator(_userManager, _roleManager);
            var validationResult = await validator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Message = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                };
            }

            var userName = registerDto.Email;

            var newUser = new ApplicationUser
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                UserName = userName,
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender,
                Latitude = registerDto.Latitude,
                Longitude = registerDto.Longitude,
                DateOfRegistration = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    IsAuthenticated = false
                };
            }

            await _userManager.AddToRoleAsync(newUser, registerDto.RoleName);
            
            var roleNameUpper = registerDto.RoleName.ToUpperInvariant();
            if (roleNameUpper == "DOCTOR")
            {
                var doctor = new Doctor
                {
                    Id = newUser.Id,
                    Specialization = string.Empty,
                    IdNo = string.Empty
                };
                await _unitOfWork.Doctors.Add(doctor);
                await _unitOfWork.SaveChanges();

                for (int i = 0; i <= 6; i++)
                {
                    await _unitOfWork.DoctorSchedules.Add(new DoctorSchedule
                    {
                        DoctorId = newUser.Id,
                        DayOfWeek = (DayOfWeek)i,
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)),
                        SlotDurationMinutes = 30,
                        IsActive = true
                    });
                }
                await _unitOfWork.SaveChanges();
            }
            else if (roleNameUpper == "PATIENT")
            {
                var patient = new Patient
                {
                    Id = newUser.Id
                };
                await _unitOfWork.Patients.Add(patient);
                await _unitOfWork.SaveChanges();
            }
            
            var token = await CreateJwtToken(newUser);
            var refreshToken = GenerateRefreshToken();
            refreshToken.UserId = newUser.Id;
            await _unitOfWork.RefreshTokens.Add(refreshToken);
            await _unitOfWork.SaveChanges();

            return new AuthResponseDto
            {
                Message = "User registered successfully",
                Token = token,
                IsAuthenticated = true,
                UserName = newUser.UserName,
                Email = newUser.Email,
                Roles = new List<string> { registerDto.RoleName },
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresOn = refreshToken.ExpiresOn
            };
        }

        private async Task<RefreshToken> GetActiveRefreshToken(ApplicationUser user)
        {
            var activeRefreshToken = await _unitOfWork.RefreshTokens.Find(t =>
                t.UserId == user.Id &&
                t.ExpiresOn > DateTime.UtcNow &&
                t.RevokedOn == null);

            if (activeRefreshToken is not null)
                return activeRefreshToken;

            var newRefreshToken = GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokens.Add(newRefreshToken);
            await _unitOfWork.SaveChanges();
            return newRefreshToken;
        }

        private async Task<string> CreateJwtToken(ApplicationUser user)
        {
            IList<Claim> userClaims;
            try
            {
                userClaims = await _userManager.GetClaimsAsync(user);
            }
            catch (InvalidOperationException)
            {
                userClaims = new List<Claim>();
            }
            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            if (roles.Count == 0)
            {
                var hasPatient = await _unitOfWork.Patients.Find(p => p.Id == user.Id) != null;
                var hasDoctor = await _unitOfWork.Doctors.Find(d => d.Id == user.Id) != null;
                if (hasPatient) roles.Add(Roles.Patient);
                else if (hasDoctor) roles.Add(Roles.Doctor);
            }

            var claims = new List<Claim>();
            var permissionValues = new List<string>();

            foreach (var roleName in roles)
            {
                claims.Add(new Claim("role", roleName));
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is not null)
                {
                    try
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        foreach (var rc in roleClaims.Where(c => c?.Type == ClaimConstants.Permission && !string.IsNullOrEmpty(c.Value)))
                            permissionValues.Add(rc.Value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            if (permissionValues.Count > 0)
                claims.Add(new Claim(ClaimConstants.Permission, string.Join(",", permissionValues.Distinct())));

            var registeredTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                JwtRegisteredClaimNames.Jti,
                JwtRegisteredClaimNames.Sub,
                JwtRegisteredClaimNames.UniqueName,
                JwtRegisteredClaimNames.Email,
                ClaimConstants.Permission
            };
            var userClaimsFiltered = userClaims.Where(c => c?.Type != null && !registeredTypes.Contains(c.Type)).ToList();

            var allClaims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
                }
                .Union(userClaimsFiltered)
                .Union(claims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                allClaims,
                expires: DateTime.UtcNow.AddYears(_jwt.ExpirationInYears),
                signingCredentials: signingCredentials);

            try
            {
                return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            }
            catch (InvalidOperationException)
            {
                var minimalClaims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Sub, user.Id),
                    new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                    new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
                };
                foreach (var roleName in roles)
                    minimalClaims.Add(new Claim("role", roleName));
                var minimalToken = new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    minimalClaims,
                    expires: DateTime.UtcNow.AddYears(_jwt.ExpirationInYears),
                    signingCredentials: signingCredentials);
                return new JwtSecurityTokenHandler().WriteToken(minimalToken);
            }
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationInDays),
                CreatedOn = DateTime.UtcNow
            };
        }

        public async Task<UserProfileDto?> GetUserProfile(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var isDoctor = user.Doctor != null;
            var isPatient = user.Patient != null;

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                ProfileImageUrl = user.ProfileImageUrl,
                DateOfRegistration = user.DateOfRegistration,
                Roles = roles.ToList(),
                IsDoctor = isDoctor,
                IsPatient = isPatient,
                Specialization = user.Doctor?.Specialization,
                IdNo = user.Doctor?.IdNo
            };
        }

        public async Task<(string Token, string RefreshToken, DateTime RefreshTokenExpiresOn)> GenerateTokensForUser(ApplicationUser user)
        {
            var token = await CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            refreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokens.Add(refreshToken);
            await _unitOfWork.SaveChanges();
            return (token, refreshToken.Token, refreshToken.ExpiresOn);
        }

        public async Task<(bool Success, string Message)> RequestChangePasswordCodeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "User not found.");

            var email = user.Email?.Trim();
            if (string.IsNullOrEmpty(email))
                return (false, "Your account has no email. Cannot send change-password code.");

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var key = ChangePasswordCacheKeyPrefix + userId;
            _cache.Set(key, code, ChangePasswordCodeExpiry);

            var subject = "Tabibak – Change password code";
            var body = EmailTemplates.ChangePasswordCode(code, "5 minutes");
            var sent = await _emailService.SendEmailAsync(email, subject, body, isHtml: true);
            if (!sent)
                return (false, "Failed to send the code to your email. Please try again.");

            return (true, "A 6-digit code has been sent to your email. Use it with your current password and new password to change password.");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (dto == null)
                return (false, "Request body is required (code, currentPassword, newPassword).");
            if (string.IsNullOrWhiteSpace(dto.Code))
                return (false, "Verification code is required.");
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return (false, "Current password is required.");
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return (false, "New password is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "User not found.");

            var key = ChangePasswordCacheKeyPrefix + userId;
            var codeMatch = _cache.TryGetValue(key, out object? cached) &&
                           cached != null &&
                           string.Equals(cached.ToString(), dto.Code.Trim(), StringComparison.Ordinal);
            if (!codeMatch)
                return (false, "Invalid or expired code. Please request a new code from change-password/request-code.");

            var checkPassword = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!checkPassword)
                return (false, "Current (old) password is incorrect.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

            _cache.Remove(key);

            var email = user.Email ?? user.UserName;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    var subject = "Tabibak – Password changed";
                    var body = EmailTemplates.PasswordChanged();
                    await _emailService.SendEmailAsync(email.Trim(), subject, body, isHtml: true);
                }
                catch { /* don't fail the request */ }
            }

            return (true, "Password changed successfully. A confirmation email has been sent.");
        }

        public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null)
                return (false, "No account found with this email.");

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var key = ForgotPasswordCacheKeyPrefix + email.Trim().ToLowerInvariant();
            _cache.Set(key, code, ForgotPasswordCodeExpiry);

            var subject = "Tabibak – Password reset code";
            var body = EmailTemplates.ForgotPasswordCode(code, "5 minutes");
            var sent = await _emailService.SendEmailAsync(email.Trim(), subject, body, isHtml: true);
            if (!sent)
                return (false, "Failed to send the code to your email. Please try again.");

            return (true, "A 6-digit code has been sent to your email. Use it to reset your password.");
        }

        public async Task<(bool Success, string Message)> VerifyAndResetPasswordAsync(ForgotPasswordVerifyDto? dto)
        {
            try
            {
                if (dto == null)
                    return (false, "Request body is required (email, code, newPassword).");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return (false, "Email is required.");
                if (string.IsNullOrWhiteSpace(dto.Code))
                    return (false, "Verification code is required.");
                if (string.IsNullOrWhiteSpace(dto.NewPassword))
                    return (false, "New password is required.");

                var email = dto.Email.Trim();
                var key = ForgotPasswordCacheKeyPrefix + email.ToLowerInvariant();
                var codeMatch = _cache.TryGetValue(key, out object? cached) &&
                                cached != null &&
                                string.Equals(cached.ToString(), dto.Code.Trim(), StringComparison.Ordinal);
                if (!codeMatch)
                    return (false, "Invalid or expired code. Please request a new code.");

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return (false, "No account found with this email.");

                IdentityResult result;
                try
                {
                    var removeResult = await _userManager.RemovePasswordAsync(user);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogWarning("RemovePassword failed for {Email}: {Errors}", email, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        return (false, "Password reset failed. Please request a new code and try again.");
                    }
                    result = await _userManager.AddPasswordAsync(user, dto.NewPassword);
                }
                catch (Exception resetEx)
                {
                    _logger.LogError(resetEx, "Password reset (RemovePassword/AddPassword) failed for {Email}", email);
                    return (false, "Password reset failed. Please request a new code and try again.");
                }

                if (!result.Succeeded)
                    return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

                _cache.Remove(key);

                try
                {
                    var subject = "Tabibak – Password changed";
                    var body = EmailTemplates.PasswordChanged();
                    await _emailService.SendEmailAsync(email, subject, body, isHtml: true);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Forgot-password verify: confirmation email failed for {Email}", email);
                }

                return (true, "Password has been reset successfully. You can now log in with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPasswordVerify failed for email {Email}", dto?.Email ?? "(null)");
                return (false, "A temporary error occurred. Please try again or request a new code.");
            }
        }

        public async Task CachePendingRegistrationAsync(string email, string registrationJson)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            var trimmed = email.Trim();
            var emailKey = trimmed.ToLowerInvariant();

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            _cache.Set(EmailVerifyCacheKeyPrefix + emailKey, otpCode, EmailVerifyCodeExpiry);
            _cache.Set(PendingRegCacheKeyPrefix + emailKey, registrationJson, EmailVerifyCodeExpiry);

            var subject = "Tabibak – Verify your email";
            var body = EmailTemplates.EmailVerificationCode(otpCode, "60 seconds");
            await _emailService.SendEmailAsync(trimmed, subject, body, isHtml: true);
        }

        public Task<(bool Success, string Message, string? RegistrationJson)> VerifyOtpAndGetPendingAsync(VerifyEmailOtpDto dto)
        {
            if (dto == null)
                return Task.FromResult<(bool, string, string?)>((false, "Request body is required.", null));
            if (string.IsNullOrWhiteSpace(dto.Email))
                return Task.FromResult<(bool, string, string?)>((false, "Email is required.", null));
            if (string.IsNullOrWhiteSpace(dto.Code))
                return Task.FromResult<(bool, string, string?)>((false, "Verification code is required.", null));

            var emailKey = dto.Email.Trim().ToLowerInvariant();
            var otpCacheKey = EmailVerifyCacheKeyPrefix + emailKey;
            var regCacheKey = PendingRegCacheKeyPrefix + emailKey;

            var codeMatch = _cache.TryGetValue(otpCacheKey, out object? cached) &&
                            cached != null &&
                            string.Equals(cached.ToString(), dto.Code.Trim(), StringComparison.Ordinal);
            if (!codeMatch)
                return Task.FromResult<(bool, string, string?)>((false, "Invalid or expired code.", null));

            if (!_cache.TryGetValue(regCacheKey, out object? regData) || regData == null)
                return Task.FromResult<(bool, string, string?)>((false, "Registration data expired. Please register again.", null));

            _cache.Remove(otpCacheKey);
            _cache.Remove(regCacheKey);

            return Task.FromResult<(bool, string, string?)>((true, "Email verified.", regData.ToString()));
        }
    }
}
