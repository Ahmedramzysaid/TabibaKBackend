namespace ClinicAPI.Services;

public interface IDoctorVerificationService
{
    Task<(bool IsVerified, string? Specialization)> VerifyDoctorNameAsync(string arabicName);
}
