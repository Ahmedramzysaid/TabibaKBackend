namespace DomainLayer.Models;

public class DoctorSchedule
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; } = 30;

    public bool IsActive { get; set; } = true;

    public Doctor Doctor { get; set; } = null!;
}
