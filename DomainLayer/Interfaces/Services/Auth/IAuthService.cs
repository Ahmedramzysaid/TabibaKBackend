using DomainLayer.DTOs;
using DomainLayer.Models;

namespace DomainLayer.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto registerDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
        Task<AuthResponseDto> RefreshToken(string token);
        Task<bool> RevokeToken(string token);
        Task<UserProfileDto?> GetUserProfile(string userId);
        Task<(string Token, string RefreshToken, DateTime RefreshTokenExpiresOn)> GenerateTokensForUser(ApplicationUser user);
        Task<(bool Success, string Message)> RequestChangePasswordCodeAsync(string userId);
        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);
        Task<(bool Success, string Message)> VerifyAndResetPasswordAsync(ForgotPasswordVerifyDto dto);
        Task CachePendingRegistrationAsync(string email, string registrationJson);
        Task<(bool Success, string Message, string? RegistrationJson)> VerifyOtpAndGetPendingAsync(VerifyEmailOtpDto dto);
    }
}
