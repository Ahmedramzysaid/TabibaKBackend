namespace DomainLayer.DTOs;

public class SetDoctorScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; } = 30;

    public bool IsActive { get; set; } = true;
}
