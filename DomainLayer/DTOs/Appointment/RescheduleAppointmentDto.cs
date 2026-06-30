namespace DomainLayer.DTOs;

public class RescheduleAppointmentDto
{
    public Guid AppointmentID { get; set; }
    public DateOnly NewAppointmentDate { get; set; }
    public TimeSpan NewAppointmentTime { get; set; }
    public string? DoctorID { get; set; }
}
