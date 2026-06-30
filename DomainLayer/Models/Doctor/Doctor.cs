namespace DomainLayer.Models;

public class Doctor
{
    public string Id { get; set; } = string.Empty;
    
    public string Specialization { get; set; } = string.Empty;
    public string IdNo { get; set; } = string.Empty;
    
    public string? IdImageFrontUrl { get; set; }
    public string? IdImageBackUrl { get; set; }

    public decimal? Price { get; set; }

    public bool IsAvailable { get; set; }

    public decimal? Rating { get; set; }
    public int RatingCount { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    
    public ApplicationUser User { get; set; } = null!;
    
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public ICollection<DoctorAdviceVideo> DoctorAdviceVideos { get; set; } = new List<DoctorAdviceVideo>();

    public ICollection<DoctorAdvicePost> AdvicePosts { get; set; } = new List<DoctorAdvicePost>();

    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();

    public ICollection<DoctorEarning> Earnings { get; set; } = new List<DoctorEarning>();

    public DomainLayer.Enums.DoctorVerificationStatus VerificationStatus { get; set; } = DomainLayer.Enums.DoctorVerificationStatus.Pending;

    public string? RejectionReason { get; set; }
}
