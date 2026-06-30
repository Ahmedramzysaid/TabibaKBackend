namespace DomainLayer.DTOs;

public class CompleteAppointmentDto
{
    public Guid AppointmentID { get; set; }
    public CompletePrescriptionDto CompletePrescriptionDto { get; set; }
    public CompletePaymentDto CompletePaymentDto { get; set; }
}
