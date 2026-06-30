using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        private ActionResult MapResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Data);

            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(new { message = result.Message }),
                ServiceErrorType.ValidationError => BadRequest(new { message = result.Message }),
                ServiceErrorType.Conflict => Conflict(new { message = result.Message }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Message })
            };
        }


        [HttpGet("overview")]
        public async Task<ActionResult<AdminSystemOverviewDto>> GetOverview()
        {
            var result = await _adminDashboardService.GetSystemOverviewAsync();
            return MapResult(result);
        }

        [HttpGet("analytics/registrations")]
        public async Task<ActionResult<IEnumerable<AdminRegistrationTrendDto>>> GetRegistrationTrends([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _adminDashboardService.GetRegistrationTrendsAsync(from, to);
            return MapResult(result);
        }

        [HttpGet("analytics/appointments")]
        public async Task<ActionResult<IEnumerable<AdminAppointmentTrendDto>>> GetAppointmentTrends([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _adminDashboardService.GetAppointmentTrendsAsync(from, to);
            return MapResult(result);
        }

        [HttpGet("analytics/revenue")]
        public async Task<ActionResult<IEnumerable<AdminRevenueTrendDto>>> GetRevenueTrends([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string groupBy = "day")
        {
            var result = await _adminDashboardService.GetRevenueTrendsAsync(from, to, groupBy);
            return MapResult(result);
        }

        [HttpGet("analytics/top-specializations")]
        public async Task<ActionResult<IEnumerable<AdminTopSpecializationDto>>> GetTopSpecializations([FromQuery] int count = 10)
        {
            var result = await _adminDashboardService.GetTopSpecializationsAsync(count);
            return MapResult(result);
        }


        [HttpGet("users")]
        public async Task<ActionResult<PaginatedResult<AdminUserListItemDto>>> GetAllUsers([FromQuery] AdminUserFilterDto filter)
        {
            var result = await _adminDashboardService.GetAllUsersAsync(filter);
            return MapResult(result);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<AdminUserDetailDto>> GetUserDetail(string id)
        {
            var result = await _adminDashboardService.GetUserDetailAsync(id);
            return MapResult(result);
        }

        [HttpPost("users/{id}/ban")]
        public async Task<ActionResult> BanUser(string id, [FromBody] AdminBanUserDto dto)
        {
            var result = await _adminDashboardService.BanUserAsync(id, dto.Reason);
            if (result.IsSuccess) return Ok(new { message = "User banned successfully." });
            return MapResult(result);
        }

        [HttpPost("users/{id}/activate")]
        public async Task<ActionResult> ActivateUser(string id)
        {
            var result = await _adminDashboardService.ActivateUserAsync(id);
            if (result.IsSuccess) return Ok(new { message = "User activated successfully." });
            return MapResult(result);
        }

        [HttpPut("users/{id}/role")]
        public async Task<ActionResult> ChangeUserRole(string id, [FromBody] AdminChangeUserRoleDto dto)
        {
            var result = await _adminDashboardService.ChangeUserRoleAsync(id, dto);
            if (result.IsSuccess) return Ok(new { message = "User role updated successfully." });
            return MapResult(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var result = await _adminDashboardService.DeleteUserAsync(id);
            if (result.IsSuccess) return Ok(new { message = "User deleted successfully." });
            return MapResult(result);
        }


        [HttpGet("doctors")]
        public async Task<ActionResult<PaginatedResult<AdminDoctorListItemDto>>> GetAllDoctors([FromQuery] AdminDoctorFilterDto filter)
        {
            var result = await _adminDashboardService.GetAllDoctorsAsync(filter);
            return MapResult(result);
        }

        [HttpPost("doctors/{id}/verify")]
        public async Task<ActionResult> VerifyDoctor(string id)
        {
            var result = await _adminDashboardService.VerifyDoctorAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Doctor verified successfully." });
            return MapResult(result);
        }

        [HttpPost("doctors/{id}/reject")]
        public async Task<ActionResult> RejectDoctor(string id, [FromBody] AdminRejectDoctorDto dto)
        {
            var result = await _adminDashboardService.RejectDoctorAsync(id, dto.Reason);
            if (result.IsSuccess) return Ok(new { message = "Doctor rejected successfully." });
            return MapResult(result);
        }

        [HttpPut("doctors/{id}/force-price")]
        public async Task<ActionResult> ForceDoctorPrice(string id, [FromBody] AdminForcePriceDto dto)
        {
            var result = await _adminDashboardService.ForceDoctorPriceAsync(id, dto.Price);
            if (result.IsSuccess) return Ok(new { message = "Doctor price updated successfully." });
            return MapResult(result);
        }


        [HttpGet("patients")]
        public async Task<ActionResult<PaginatedResult<AdminPatientListItemDto>>> GetAllPatients([FromQuery] AdminUserFilterDto filter)
        {
            var result = await _adminDashboardService.GetAllPatientsAsync(filter);
            return MapResult(result);
        }


        [HttpGet("appointments")]
        public async Task<ActionResult<PaginatedResult<AdminAppointmentListItemDto>>> GetAllAppointments([FromQuery] AdminAppointmentFilterDto filter)
        {
            var result = await _adminDashboardService.GetAllAppointmentsAsync(filter);
            return MapResult(result);
        }

        [HttpPut("appointments/{id}/force-cancel")]
        public async Task<ActionResult> ForceCancelAppointment(Guid id, [FromBody] AdminForceCancelDto dto)
        {
            var result = await _adminDashboardService.ForceCancelAppointmentAsync(id, dto.Reason);
            if (result.IsSuccess) return Ok(new { message = "Appointment canceled successfully." });
            return MapResult(result);
        }

        [HttpPut("appointments/{id}/force-complete")]
        public async Task<ActionResult> ForceCompleteAppointment(Guid id)
        {
            var result = await _adminDashboardService.ForceCompleteAppointmentAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Appointment completed successfully." });
            return MapResult(result);
        }


        [HttpGet("content/posts")]
        public async Task<ActionResult<PaginatedResult<AdminAdvicePostListDto>>> GetAllAdvicePosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _adminDashboardService.GetAllAdvicePostsAsync(page, pageSize);
            return MapResult(result);
        }

        [HttpDelete("content/posts/{id}")]
        public async Task<ActionResult> DeleteAdvicePost(Guid id)
        {
            var result = await _adminDashboardService.DeleteAdvicePostAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Post deleted successfully." });
            return MapResult(result);
        }

        [HttpPut("content/posts/{id}/toggle-publish")]
        public async Task<ActionResult> TogglePostPublish(Guid id)
        {
            var result = await _adminDashboardService.TogglePostPublishAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Post publish status toggled successfully." });
            return MapResult(result);
        }

        [HttpGet("content/videos")]
        public async Task<ActionResult<PaginatedResult<AdminAdviceVideoListDto>>> GetAllAdviceVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _adminDashboardService.GetAllAdviceVideosAsync(page, pageSize);
            return MapResult(result);
        }

        [HttpDelete("content/videos/{id}")]
        public async Task<ActionResult> DeleteAdviceVideo(Guid id)
        {
            var result = await _adminDashboardService.DeleteAdviceVideoAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Video deleted successfully." });
            return MapResult(result);
        }

        [HttpPut("content/videos/{id}/toggle-publish")]
        public async Task<ActionResult> ToggleVideoPublish(Guid id)
        {
            var result = await _adminDashboardService.ToggleVideoPublishAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Video publish status toggled successfully." });
            return MapResult(result);
        }


        [HttpGet("finance/summary")]
        public async Task<ActionResult<AdminFinancialSummaryDto>> GetFinancialSummary()
        {
            var result = await _adminDashboardService.GetFinancialSummaryAsync();
            return MapResult(result);
        }

        [HttpGet("finance/revenue-by-doctor")]
        public async Task<ActionResult<IEnumerable<AdminDoctorRevenueDto>>> GetRevenueByDoctor([FromQuery] int count = 20)
        {
            var result = await _adminDashboardService.GetRevenueByDoctorAsync(count);
            return MapResult(result);
        }

        [HttpGet("finance/revenue-trend")]
        public async Task<ActionResult<IEnumerable<AdminRevenueTrendDto>>> GetFinancialRevenueTrend([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string groupBy = "day")
        {
            var result = await _adminDashboardService.GetRevenueTrendsAsync(from, to, groupBy);
            return MapResult(result);
        }


        [HttpGet("system/recent-activity")]
        public async Task<ActionResult<IEnumerable<AdminRecentActivityDto>>> GetRecentActivity([FromQuery] int count = 50)
        {
            var result = await _adminDashboardService.GetRecentActivityAsync(count);
            return MapResult(result);
        }

        [HttpGet("system/ratings")]
        public async Task<ActionResult<PaginatedResult<AdminDoctorRatingListDto>>> GetAllRatings([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _adminDashboardService.GetAllRatingsAsync(page, pageSize);
            return MapResult(result);
        }

        [HttpDelete("system/ratings/{id}")]
        public async Task<ActionResult> DeleteRating(int id)
        {
            var result = await _adminDashboardService.DeleteRatingAsync(id);
            if (result.IsSuccess) return Ok(new { message = "Rating deleted successfully." });
            return MapResult(result);
        }
    }
}
