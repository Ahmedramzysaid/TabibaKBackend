namespace DomainLayer.DTOs;

public class RescheduleAppointmentRequestDto
{
    public Guid AppointmentID { get; set; }
    public DateOnly NewAppointmentDate { get; set; }
    public TimeSpan NewAppointmentTime { get; set; }
}
