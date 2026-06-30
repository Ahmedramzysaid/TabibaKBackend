namespace DomainLayer.DTOs;

public class SubmitDoctorRatingDto
{
    public Guid AppointmentID { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
}
