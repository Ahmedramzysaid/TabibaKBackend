using System;
using System.Collections.Generic;
using DomainLayer.Enums;

namespace DomainLayer.DTOs
{

    public class AdminSystemOverviewDto
    {
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalCompletedAppointments { get; set; }
        public int TotalPendingAppointments { get; set; }
        public int TotalCanceledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalAdvicePosts { get; set; }
        public int TotalAdviceVideos { get; set; }
        public int TotalConversations { get; set; }
        public int TotalMedicines { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int AppointmentsToday { get; set; }
        public int AppointmentsThisWeek { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
    }

    public class AdminRegistrationTrendDto
    {
        public DateTime Date { get; set; }
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
    }

    public class AdminAppointmentTrendDto
    {
        public DateTime Date { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Rescheduled { get; set; }
    }

    public class AdminRevenueTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class AdminTopSpecializationDto
    {
        public string Specialization { get; set; } = string.Empty;
        public int DoctorCount { get; set; }
        public int AppointmentCount { get; set; }
    }


    public class AdminUserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfRegistration { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public bool IsDoctor { get; set; }
        public bool IsPatient { get; set; }
        public string? DoctorSpecialization { get; set; }
        public string? DoctorVerificationStatus { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class AdminUserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? Gender { get; set; }
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AdminUserDetailDto : AdminUserListItemDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }

        public decimal? DoctorPrice { get; set; }
        public decimal? DoctorRating { get; set; }
        public int DoctorRatingCount { get; set; }
        public bool DoctorIsAvailable { get; set; }
        public string? DoctorIdNo { get; set; }
        public string? DoctorIdImageFrontUrl { get; set; }
        public string? DoctorIdImageBackUrl { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalAdvicePosts { get; set; }
        public int TotalAdviceVideos { get; set; }

        public Guid? MedicalRecordId { get; set; }
        public int TotalPrescriptions { get; set; }
    }

    public class AdminBanUserDto
    {
        public string? Reason { get; set; }
    }

    public class AdminChangeUserRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "Add" or "Remove"
    }


    public class AdminDoctorListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal? Rating { get; set; }
        public int RatingCount { get; set; }
        public bool IsAvailable { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalAdvicePosts { get; set; }
        public int TotalAdviceVideos { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime DateOfRegistration { get; set; }
    }

    public class AdminDoctorFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Specialization { get; set; }
        public string? VerificationStatus { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? MinRating { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AdminRejectDoctorDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class AdminForcePriceDto
    {
        public decimal Price { get; set; }
    }


    public class AdminPatientListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfRegistration { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }
        public bool HasMedicalRecord { get; set; }
        public bool IsActive { get; set; }
    }


    public class AdminAppointmentListItemDto
    {
        public Guid AppointmentId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
        public DateOnly AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasMedicalRecord { get; set; }
        public bool HasPayment { get; set; }
        public float? PaymentAmount { get; set; }
        public string? AdditionalNotes { get; set; }
    }

    public class AdminAppointmentFilterDto
    {
        public string? SearchTerm { get; set; }
        public short? Status { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AdminForceCancelDto
    {
        public string? Reason { get; set; }
    }


    public class AdminAdvicePostListDto
    {
        public Guid Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class AdminAdviceVideoListDto
    {
        public Guid Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }
    }


    public class AdminFinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }
        public decimal AverageRevenuePerDay { get; set; }
        public AdminTopEarnerDto? TopEarningDoctor { get; set; }
        public int TotalPayments { get; set; }
    }

    public class AdminTopEarnerDto
    {
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
    }

    public class AdminDoctorRevenueDto
    {
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal AveragePerAppointment { get; set; }
    }


    public class AdminRecentActivityDto
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? EntityId { get; set; }
    }

    public class AdminDoctorRatingListDto
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid AppointmentId { get; set; }
    }
}
