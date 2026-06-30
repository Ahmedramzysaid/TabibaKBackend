namespace DomainLayer.DTOs;

public class DashboardStatsDto
{
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CanceledAppointments { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public List<AppointmentWithDetailsDto> UpcomingAppointments { get; set; } = new();
    public List<AppointmentWithDetailsDto> RecentAppointments { get; set; } = new();
}

