using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<Result<AdminSystemOverviewDto>> GetSystemOverviewAsync();

        Task<Result<IEnumerable<AdminRegistrationTrendDto>>> GetRegistrationTrendsAsync(DateTime from, DateTime to);
        Task<Result<IEnumerable<AdminAppointmentTrendDto>>> GetAppointmentTrendsAsync(DateTime from, DateTime to);
        Task<Result<IEnumerable<AdminRevenueTrendDto>>> GetRevenueTrendsAsync(DateTime from, DateTime to, string groupBy = "day");
        Task<Result<IEnumerable<AdminTopSpecializationDto>>> GetTopSpecializationsAsync(int count = 10);

        Task<Result<PaginatedResult<AdminUserListItemDto>>> GetAllUsersAsync(AdminUserFilterDto filter);
        Task<Result<AdminUserDetailDto>> GetUserDetailAsync(string userId);
        Task<Result<bool>> BanUserAsync(string userId, string? reason);
        Task<Result<bool>> ActivateUserAsync(string userId);
        Task<Result<bool>> ChangeUserRoleAsync(string userId, AdminChangeUserRoleDto dto);
        Task<Result<bool>> DeleteUserAsync(string userId);

        Task<Result<PaginatedResult<AdminDoctorListItemDto>>> GetAllDoctorsAsync(AdminDoctorFilterDto filter);
        Task<Result<bool>> VerifyDoctorAsync(string doctorId);
        Task<Result<bool>> RejectDoctorAsync(string doctorId, string reason);
        Task<Result<bool>> ForceDoctorPriceAsync(string doctorId, decimal price);

        Task<Result<PaginatedResult<AdminPatientListItemDto>>> GetAllPatientsAsync(AdminUserFilterDto filter);

        Task<Result<PaginatedResult<AdminAppointmentListItemDto>>> GetAllAppointmentsAsync(AdminAppointmentFilterDto filter);
        Task<Result<bool>> ForceCancelAppointmentAsync(Guid appointmentId, string? reason);
        Task<Result<bool>> ForceCompleteAppointmentAsync(Guid appointmentId);

        Task<Result<PaginatedResult<AdminAdvicePostListDto>>> GetAllAdvicePostsAsync(int page, int pageSize);
        Task<Result<bool>> DeleteAdvicePostAsync(Guid postId);
        Task<Result<bool>> TogglePostPublishAsync(Guid postId);
        Task<Result<PaginatedResult<AdminAdviceVideoListDto>>> GetAllAdviceVideosAsync(int page, int pageSize);
        Task<Result<bool>> DeleteAdviceVideoAsync(Guid videoId);
        Task<Result<bool>> ToggleVideoPublishAsync(Guid videoId);

        Task<Result<AdminFinancialSummaryDto>> GetFinancialSummaryAsync();
        Task<Result<IEnumerable<AdminDoctorRevenueDto>>> GetRevenueByDoctorAsync(int count = 20);

        Task<Result<IEnumerable<AdminRecentActivityDto>>> GetRecentActivityAsync(int count = 50);
        Task<Result<PaginatedResult<AdminDoctorRatingListDto>>> GetAllRatingsAsync(int page, int pageSize);
        Task<Result<bool>> DeleteRatingAsync(int ratingId);
    }
}
