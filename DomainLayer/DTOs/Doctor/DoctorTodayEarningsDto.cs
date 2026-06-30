namespace DomainLayer.DTOs;

public class DoctorTodayEarningsDto
{
    public DateTime Date { get; set; }
    public decimal TotalEarnings { get; set; }
    public int CompletedAppointmentsCount { get; set; }
}
