namespace DomainLayer.Models;

public class DoctorEarning
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public decimal DailyEarnings { get; set; }

    public int AppointmentCount { get; set; }

    public Doctor Doctor { get; set; } = null!;
}
