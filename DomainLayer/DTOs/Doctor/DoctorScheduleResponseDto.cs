namespace DomainLayer.DTOs;

public class DoctorScheduleResponseDto
{
    public int Id { get; set; }
    public string DayName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsActive { get; set; }
}
