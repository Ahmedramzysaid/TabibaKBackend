using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.Register(registerDto);

            if (response.IsAuthenticated is false)
                return BadRequest(response);

            if (response.RefreshToken != null)
                SetTokenCookie(response.RefreshToken, response.RefreshTokenExpiresOn);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.Login(loginDto);

            if (response.IsAuthenticated is false)
                return BadRequest(response);

            if (response.RefreshToken != null)
                SetTokenCookie(response.RefreshToken, response.RefreshTokenExpiresOn);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/request")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordRequestDto dto)
        {
            var (success, message) = await _authService.RequestPasswordResetAsync(dto.Email);
            if (!success)
                return BadRequest(message);
            return Ok(new { message });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/verify")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] ForgotPasswordVerifyDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState.IsValid ? "Request body is required." : ModelState);
            try
            {
                var (success, message) = await _authService.VerifyAndResetPasswordAsync(dto);
                if (!success)
                    return BadRequest(message);
                return Ok(new { message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while resetting the password. Please try again.");
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-admin-password")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetAdminPassword()
        {
            try
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                var adminEmail = "admin@example.com";
                var adminPassword = "StrongPassword123###";
                
                var user = await userManager.FindByEmailAsync(adminEmail);
                
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        FullName = "Admin User",
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        DateOfBirth = new DateTime(1980, 1, 1),
                        Gender = "Male",
                        DateOfRegistration = DateTime.UtcNow,
                        Latitude = 0,
                        Longitude = 0
                    };
                    
                    var createResult = await userManager.CreateAsync(user, adminPassword);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                    
                    var adminRole = await roleManager.FindByNameAsync("SuperAdmin");
                    if (adminRole?.Name != null)
                    {
                        await userManager.AddToRoleAsync(user, adminRole.Name);
                    }
                    
                    return Ok("Admin user created successfully. Password: StrongPassword123###");
                }
                else
                {
                    await userManager.RemovePasswordAsync(user);
                    var result = await userManager.AddPasswordAsync(user, adminPassword);
                    
                    if (result.Succeeded)
                    {
                        var adminRole = await roleManager.FindByNameAsync("SuperAdmin");
                        if (adminRole?.Name != null)
                        {
                            var userRoles = await userManager.GetRolesAsync(user);
                            if (!userRoles.Contains(adminRole.Name))
                            {
                                await userManager.AddToRoleAsync(user, adminRole.Name);
                            }
                        }
                        return Ok("Admin password has been reset successfully and assigned to SuperAdmin role. Password: StrongPassword123###");
                    }
                    else
                    {
                        return BadRequest($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private void SetTokenCookie(string token, DateTime expirationDate)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expirationDate.ToLocalTime()
            };
            Response.Cookies.Append("RefreshToken", token, cookieOptions);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Invalid token");
            var response = await _authService.RefreshToken(refreshToken);
            if (response.IsAuthenticated is false)
                return BadRequest(response.Message);
            if (response.RefreshToken != null)
                SetTokenCookie(response.RefreshToken, response.RefreshTokenExpiresOn);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("revoke-token")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto revokeTokenDto)
        {
            var token = revokeTokenDto.Token ?? Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token");
            var response = await _authService.RevokeToken(token);
            if (response is false)
                return BadRequest("Invalid token");
            return Ok(response);
        }

        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var profile = await _authService.GetUserProfile(userId);
            if (profile == null)
                return NotFound("User profile not found");

            return Ok(profile);
        }

        [Authorize]
        [HttpPost("change-password/request-code")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RequestChangePasswordCode()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (success, message) = await _authService.RequestChangePasswordCodeAsync(userId);
            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _authService.ChangePasswordAsync(userId, dto);
            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }
    }
}
