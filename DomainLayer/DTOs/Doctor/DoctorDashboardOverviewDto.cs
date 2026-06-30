namespace DomainLayer.DTOs;

public class DoctorDashboardOverviewDto
{
    public string DoctorId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? ProfileImageUrl { get; set; }

    public int TodayAppointmentsCount { get; set; }
    public int CompletedTodayCount { get; set; }
    public int PendingTodayCount { get; set; }

    public int ActivePatientsCount { get; set; }
    public int PendingReportsCount { get; set; }
    public decimal TotalEarningsEgp { get; set; }
    public decimal TodayEarningsEgp { get; set; }

    public decimal? Rating { get; set; }
    public int RatingCount { get; set; }

    public DoctorNextAppointmentDto? NextAppointment { get; set; }
}
