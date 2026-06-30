namespace DomainLayer.DTOs;

public class CompletePaymentDto
{
    public DateTime PaymentDate { get; set; }
    public float AmountPaid { get; set; }
    public string? AdditionalNotes { get; set; }
}
