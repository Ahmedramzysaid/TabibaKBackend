using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IDashboardDoctorService
{
    Task<Result<DoctorDashboardOverviewDto>> GetOverviewAsync(string doctorId);
    Task<Result<IEnumerable<DoctorTodayAppointmentItemDto>>> GetTodayAppointmentsAsync(string doctorId);
    Task<Result<DoctorNextAppointmentDto?>> GetNextAppointmentAsync(string doctorId);
    Task<Result<IEnumerable<DoctorRecentPatientDto>>> GetRecentPatientsAsync(string doctorId, int count = 10);
    Task<Result<bool>> SetAvailabilityAsync(string doctorId, bool isAvailable);
    Task<Result<IEnumerable<DoctorScheduledAppointmentDto>>> GetScheduledAppointmentsAsync(string doctorId);
    Task<Result<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetPastMedicalRecordsAsync(string doctorId);
    Task<Result<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetFutureMedicalRecordsAsync(string doctorId);
    Task<Result<bool>> SetPriceAsync(string doctorId, decimal price);

    Task<Result<IEnumerable<DoctorScheduleResponseDto>>> SetScheduleAsync(string doctorId, List<SetDoctorScheduleDto> schedules);
    Task<Result<IEnumerable<DoctorScheduleResponseDto>>> GetScheduleAsync(string doctorId);

    Task<Result<PaginatedResult<DoctorTodayAppointmentItemDto>>> GetNotCompletedAppointmentsAsync(string doctorId, int page = 1, int pageSize = 20);
    Task<Result<PaginatedResult<DoctorTodayAppointmentItemDto>>> GetCompletedAppointmentsAsync(string doctorId, int page = 1, int pageSize = 20);

    Task<Result<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetMedicalRecordsByPatientAsync(string doctorId, string patientId, bool completedOnly);

    Task<Result<DoctorTodayEarningsDto>> GetTodayEarningsAsync(string doctorId);

    Task<Result<DoctorFinancialReportDto>> GetFinancialReportAsync(string doctorId, DateTime from, DateTime to);

    Task UpsertDailyEarningAsync(string doctorId, DateTime date, decimal amount);
}
