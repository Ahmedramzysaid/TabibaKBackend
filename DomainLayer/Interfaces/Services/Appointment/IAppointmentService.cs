using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IAppointmentService
{
    Task<Result<CreateAppointmentResponseDto>> Add(AppointmentDto appointmentDto);
    Task<Result<CreateAppointmentResponseDto>> Reschedule(RescheduleAppointmentDto rescheduleAppointmentDto);

    Task<Result<CreateAppointmentResponseDto>> Cancel(Guid appointmentId);

    Task<Result<CompleteAppointmentDto>> Complete(CompleteAppointmentDto completeAppointmentDto);

    Task<Result<CreateAppointmentResponseDto>> CompleteByAppointmentId(Guid appointmentId);

    Task<Result<IEnumerable<AppointmentDto>>> GetAll();

    Task<Result<AppointmentDto>> GetById(Guid id);

    Task<Result<PaginatedResult<CreateAppointmentResponseDto>>> GetAppointmentsAsCreateResponse(string? patientId = null, string? doctorId = null, int pageNumber = 1, int pageSize = 10);

    Task<Result<CreateAppointmentResponseDto>> GetByIdAsCreateResponse(Guid id);

    Task<Result<IEnumerable<AppointmentListCardDto>>> GetAllListCards(string? patientId = null, string? doctorId = null);
    Task<Result<AppointmentWithDetailsDto>> GetByIdWithDetails(Guid id);
    Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetAllWithDetails();
    Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetByPatientIdWithDetails(string patientId);
    Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetByDoctorIdWithDetails(string doctorId);
    Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetUpcomingAppointments(string? doctorId = null, string? patientId = null);

    Task<Result<CreateAppointmentResponseDto>> SubmitRating(SubmitDoctorRatingDto dto, string patientId);
}
