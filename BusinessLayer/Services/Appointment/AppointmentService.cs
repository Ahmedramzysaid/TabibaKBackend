using AutoMapper;
using BusinessLayer.Validations;
using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using static DomainLayer.Helpers.AppointmentScheduling;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Persistence;

namespace BusinessLayer.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly DomainLayer.Interfaces.IEmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly IDashboardDoctorService _dashboardDoctorService;

    public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AppointmentService> logger, ApplicationDbContext context, DomainLayer.Interfaces.IEmailService emailService, IMemoryCache cache, IDashboardDoctorService dashboardDoctorService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _context = context;
        _emailService = emailService;
        _cache = cache;
        _dashboardDoctorService = dashboardDoctorService;
    }

    private ValidationsResult ValidateAppointment(AppointmentDto appointmentDto)
    {
        var validator = new AppointmentValidator();
        var validationResult = validator.Validate(appointmentDto);
        if (!validationResult.IsValid)
        {
            string message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new ValidationsResult(false, message);
        }

        return new ValidationsResult(true, "");
    }

    private async Task<ValidationsResult> ValidateEntitiesExist(string doctorId, string patientId)
    {
        bool doctorExists = await _unitOfWork.Doctors.ExistsAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return new ValidationsResult(false, $"Doctor with ID {doctorId} does not exist");

        bool patientExists = await _unitOfWork.Patients.ExistsAsync(p => p.Id == patientId);
        if (!patientExists)
            return new ValidationsResult(false, $"Patient with ID {patientId} does not exist");

        return new ValidationsResult(true);
    }


    public async Task<Result<CreateAppointmentResponseDto>> Add(AppointmentDto appointmentDto)
    {
        var validationResult = ValidateAppointment(appointmentDto);
        if (!validationResult.IsValid)
        {
            return Result<CreateAppointmentResponseDto>.Failure(validationResult.ErrorMessage,
                ServiceErrorType.ValidationError);
        }

        var entitiesExistResult = await ValidateEntitiesExist(appointmentDto.DoctorID, appointmentDto.PatientID);
        if (!entitiesExistResult.IsValid)
        {
            return Result<CreateAppointmentResponseDto>.Failure(entitiesExistResult.ErrorMessage,
                ServiceErrorType.NotFound);
        }

        var appointmentDateTimeUtc = ToUtcDateTime(appointmentDto.AppointmentDate, appointmentDto.AppointmentTime);

        var cacheKey = $"doctor-schedule-{appointmentDto.DoctorID}";
        if (!_cache.TryGetValue(cacheKey, out List<DoctorSchedule>? schedules))
        {
            schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == appointmentDto.DoctorID && s.IsActive)
                .AsNoTracking()
                .ToListAsync();
            _cache.Set(cacheKey, schedules, TimeSpan.FromMinutes(30));
        }

        if (schedules != null && schedules.Any())
        {
            var appointmentDay = appointmentDateTimeUtc.DayOfWeek;
            var appointmentTime = appointmentDto.AppointmentTime;
            var matchingSchedule = schedules.FirstOrDefault(s =>
                s.DayOfWeek == appointmentDay
                && appointmentTime >= s.StartTime
                && appointmentTime <= s.EndTime - TimeSpan.FromMinutes(s.SlotDurationMinutes));

            if (matchingSchedule == null)
            {
                return Result<CreateAppointmentResponseDto>.Failure(
                    "Doctor is not available at this day/time. Please check the doctor's schedule.",
                    ServiceErrorType.ValidationError);
            }
        }

        const int MinSlotGapMinutes = 20;
        var hasConflict = await HasAppointmentConflictAsync(
            appointmentDto.DoctorID,
            appointmentDto.AppointmentDate,
            appointmentDto.AppointmentTime,
            MinSlotGapMinutes);
        if (hasConflict)
        {
            return Result<CreateAppointmentResponseDto>.Failure(
                $"The doctor already has an appointment within {MinSlotGapMinutes} minutes of this time. " +
                $"Please choose a time at least {MinSlotGapMinutes} minutes apart from existing appointments.",
                ServiceErrorType.ValidationError);
        }

        return await SaveAppointmentAndBuildResponse(appointmentDto);
    }

    private async Task<Result<CreateAppointmentResponseDto>> SaveAppointmentAndBuildResponse(AppointmentDto appointmentDto)
    {
        var appointment = _mapper.Map<Appointment>(appointmentDto);
        appointment.AdditionalNotes = appointmentDto.AdditionalNotes;
        if (appointmentDto.MedicalRecordDto != null)
            appointment.MedicalRecord = _mapper.Map<MedicalRecord>(appointmentDto.MedicalRecordDto);
        else
            appointment.MedicalRecordId = null;

        if (appointment.AppointmentID == default)
            appointment.AppointmentID = Guid.NewGuid();

        var minutesBefore = appointmentDto.ReminderMinutesBefore ?? 0;
        if (minutesBefore > 0)
        {
            var appointmentUtc = ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime);
            appointment.ReminderAt = new DateTimeOffset(appointmentUtc, TimeSpan.Zero).AddMinutes(-minutesBefore);
            appointment.ReminderSent = false;
        }
        else
        {
            appointment.ReminderAt = null;
            appointment.ReminderSent = false;
        }

        await _unitOfWork.Appointments.Add(appointment);
        var saveSucceeded = await _unitOfWork.SaveChanges();

        if (!saveSucceeded)
        {
            return Result<CreateAppointmentResponseDto>.Failure("Failed to save the appointment to the database",
                ServiceErrorType.DatabaseError);
        }

        var doctorEntity = await _unitOfWork.Doctors.GetById(appointment.DoctorID);
        if (doctorEntity != null)
        {
            var amount = (float)(doctorEntity.Price ?? 0);
            var payment = new Payment
            {
                AmountPaid = amount,
                PaymentDate = DateTime.UtcNow,
                AdditionalNotes = null
            };
            await _unitOfWork.Payments.Add(payment);
            await _unitOfWork.SaveChanges();
            appointment.PaymentID = payment.PaymentID;
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChanges();
        }

        _mapper.Map(appointment, appointmentDto);
        appointmentDto.AppointmentID = appointment.AppointmentID;
        if (appointment.MedicalRecord != null)
            appointmentDto.MedicalRecordDto = _mapper.Map<MedicalRecordDto>(appointment.MedicalRecord);

        var patient = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == appointment.PatientID);
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == appointment.DoctorID);

        var response = MapToCreateResponse(appointment, patient, doctor);

        await SendAppointmentSuccessEmailsAsync(
            appointment.PatientID,
            appointment.DoctorID,
            response.PatientName,
            response.DoctorName,
            ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime),
            isReschedule: false);
        return Result<CreateAppointmentResponseDto>.Success(response);
    }

    private async Task SendAppointmentSuccessEmailsAsync(string patientId, string doctorId, string patientName, string doctorName, DateTime appointmentDate, bool isReschedule = false)
    {
        try
        {
            var subject = isReschedule ? "Appointment rescheduled – Tabibak" : "Appointment update – Tabibak";
            var body = isReschedule
                ? EmailTemplates.AppointmentSuccessReschedule(patientName, doctorName, appointmentDate)
                : EmailTemplates.AppointmentSuccess(patientName, doctorName, appointmentDate);
            var patientUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == patientId);
            var doctorUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == doctorId);
            if (!string.IsNullOrWhiteSpace(patientUser?.Email))
                _ = _emailService.SendEmailAsync(patientUser.Email.Trim(), subject, body, isHtml: true);
            if (!string.IsNullOrWhiteSpace(doctorUser?.Email) && doctorUser.Id != patientUser?.Id)
                _ = _emailService.SendEmailAsync(doctorUser.Email.Trim(), subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send appointment success emails");
        }
    }

    private async Task<CreateAppointmentResponseDto> BuildCreateAppointmentResponseAsync(Appointment appointment)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == appointment.PatientID);
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == appointment.DoctorID);

        return MapToCreateResponse(appointment, patient, doctor);
    }

    private static CreateAppointmentResponseDto MapToCreateResponse(
        Appointment appointment,
        Patient? patient,
        Doctor? doctor)
    {
        return new CreateAppointmentResponseDto
        {
            AppointmentID = appointment.AppointmentID,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            AppointmentStatus = appointment.AppointmentStatus,
            StatusName = appointment.AppointmentStatus switch
            {
                (short)AppointmentStatus.Pending => "Pending",
                (short)AppointmentStatus.Rescheduled => "Rescheduled",
                (short)AppointmentStatus.Canceled => "Canceled",
                (short)AppointmentStatus.Completed => "Completed",
                _ => "Unknown"
            },
            PatientName = patient?.User?.FullName ?? string.Empty,
            Age = patient?.User?.DateOfBirth != default
                ? (int?)((DateTime.UtcNow - patient!.User.DateOfBirth).Days / 365)
                : null,
            Gender = patient?.User?.Gender ?? string.Empty,
            DoctorName = doctor?.User?.FullName ?? string.Empty,
            Specialization = doctor?.Specialization ?? string.Empty,
            MedicalRecordId = patient?.MedicalRecord?.MedicalRecordId
        };
    }

    private async Task<bool> HasAppointmentConflictAsync(
        string doctorId,
        DateOnly date,
        TimeSpan time,
        int minSlotGapMinutes,
        Guid? excludeAppointmentId = null)
    {
        var target = ToUtcDateTime(date, time);
        var windowStart = target.AddMinutes(-minSlotGapMinutes);
        var windowEnd = target.AddMinutes(minSlotGapMinutes);

        var candidates = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                        && a.AppointmentStatus != (short)AppointmentStatus.Canceled
                        && a.AppointmentDate >= DateOnly.FromDateTime(windowStart)
                        && a.AppointmentDate <= DateOnly.FromDateTime(windowEnd)
                        && (!excludeAppointmentId.HasValue || a.AppointmentID != excludeAppointmentId.Value))
            .Select(a => new { a.AppointmentDate, a.AppointmentTime })
            .ToListAsync();

        return candidates.Any(a =>
        {
            var existing = ToUtcDateTime(a.AppointmentDate, a.AppointmentTime);
            return existing > windowStart && existing < windowEnd;
        });
    }

    private async Task<Result<CreateAppointmentResponseDto>?> ValidateRescheduleRequest(
        Appointment appointment,
        RescheduleAppointmentDto rescheduleAppointmentDto)
    {
        if (appointment is null)
            return Result<CreateAppointmentResponseDto>.Failure(
                "Invalid appointment id, the appointment with this id is not found",
                ServiceErrorType.NotFound);

        if (appointment.AppointmentStatus == (int)AppointmentStatus.Completed)
            return Result<CreateAppointmentResponseDto>.Failure(
                "Cannot reschedule; appointment is already completed",
                ServiceErrorType.ValidationError);
        if (appointment.AppointmentStatus == (int)AppointmentStatus.Canceled)
            return Result<CreateAppointmentResponseDto>.Failure(
                "Cannot reschedule; appointment is canceled",
                ServiceErrorType.ValidationError);

        if (!string.IsNullOrEmpty(rescheduleAppointmentDto.DoctorID))
        {
            var isDoctorExists = await _unitOfWork.Doctors.ExistsAsync(d => d.Id == rescheduleAppointmentDto.DoctorID);
            if (!isDoctorExists)
                return Result<CreateAppointmentResponseDto>.Failure(
                    "Doctor with this id does not exist",
                    ServiceErrorType.NotFound);
        }

        var currentUtc = ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime);
        var newUtc = ToUtcDateTime(
            rescheduleAppointmentDto.NewAppointmentDate,
            rescheduleAppointmentDto.NewAppointmentTime);
        if (newUtc <= currentUtc)
            return Result<CreateAppointmentResponseDto>.Failure(
                "New appointment date time must be greater than the current appointment date time",
                ServiceErrorType.ValidationError);

        return null;
    }

    public async Task<Result<CreateAppointmentResponseDto>> Reschedule(RescheduleAppointmentDto rescheduleAppointmentDto)
    {
        var appointment = await _unitOfWork.Appointments.GetById(rescheduleAppointmentDto.AppointmentID);

        var validationResult = await ValidateRescheduleRequest(appointment, rescheduleAppointmentDto);
        if (validationResult != null)
            return validationResult;

        const int MinSlotGapMinutes = 20;
        var targetDoctorId = rescheduleAppointmentDto.DoctorID ?? appointment.DoctorID;
        var hasConflict = await HasAppointmentConflictAsync(
            targetDoctorId,
            rescheduleAppointmentDto.NewAppointmentDate,
            rescheduleAppointmentDto.NewAppointmentTime,
            MinSlotGapMinutes,
            appointment.AppointmentID);
        if (hasConflict)
        {
            return Result<CreateAppointmentResponseDto>.Failure(
                $"The doctor already has an appointment within {MinSlotGapMinutes} minutes of this time. " +
                $"Please choose a time at least {MinSlotGapMinutes} minutes apart from existing appointments.",
                ServiceErrorType.ValidationError);
        }

        appointment.AppointmentStatus = (int)AppointmentStatus.Rescheduled;
        appointment.AppointmentDate = rescheduleAppointmentDto.NewAppointmentDate;
        appointment.AppointmentTime = rescheduleAppointmentDto.NewAppointmentTime;
        if (!string.IsNullOrEmpty(rescheduleAppointmentDto.DoctorID))
            appointment.DoctorID = rescheduleAppointmentDto.DoctorID;

        _unitOfWork.Appointments.Update(appointment);
        var saved = await _unitOfWork.SaveChanges();
        if (!saved)
            return Result<CreateAppointmentResponseDto>.Failure("Failed to update the appointment", ServiceErrorType.DatabaseError);

        var response = await BuildCreateAppointmentResponseAsync(appointment);
        await SendAppointmentSuccessEmailsAsync(
            appointment.PatientID,
            appointment.DoctorID,
            response.PatientName,
            response.DoctorName,
            ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime),
            isReschedule: true);
        return Result<CreateAppointmentResponseDto>.Success(response);
    }


    private ValidationsResult ValidateCancelRequest(Appointment appointment)
    {
        if (appointment is null)
            return new ValidationsResult(false,
                "Invalid appointment id, the appointment with this id is not found");

        if (appointment.AppointmentStatus == (int)AppointmentStatus.Completed)
            return new ValidationsResult(false,
                "The appointment is already completed, it cannot be canceled");

        if (appointment.AppointmentStatus == (int)AppointmentStatus.Canceled)
            return new ValidationsResult(false,
                "The appointment is already canceled");

        return new ValidationsResult(true);
    }

    public async Task<Result<CreateAppointmentResponseDto>> Cancel(Guid appointmentId)
    {
        var appointment = await _unitOfWork.Appointments.GetById(appointmentId);

        var validationResult = ValidateCancelRequest(appointment);
        if (!validationResult.IsValid)
            return Result<CreateAppointmentResponseDto>.Failure(validationResult.ErrorMessage, ServiceErrorType.ValidationError);

        appointment.AppointmentStatus = (int)AppointmentStatus.Canceled;
        _unitOfWork.Appointments.Update(appointment);
        var saved = await _unitOfWork.SaveChanges();
        if (!saved)
            return Result<CreateAppointmentResponseDto>.Failure("Failed to update the appointment", ServiceErrorType.DatabaseError);

        var response = await BuildCreateAppointmentResponseAsync(appointment);
        await SendAppointmentSuccessEmailsAsync(
            appointment.PatientID,
            appointment.DoctorID,
            response.PatientName,
            response.DoctorName,
            ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime),
            isReschedule: false);
        return Result<CreateAppointmentResponseDto>.Success(response);
    }


    private async Task<ValidationsResult> ValidateCompleteRequest(CompleteAppointmentDto completeAppointmentDto)
    {
        var completeAppointmentValidator = new CompleteAppointmentValidator(_unitOfWork);
        var validations = await completeAppointmentValidator.ValidateAsync(completeAppointmentDto);
        if (!validations.IsValid)
        {
            string message = string.Join("; ", validations.Errors.Select(e => e.ErrorMessage));
            return new ValidationsResult(false, message);
        }

        return new ValidationsResult(true);
    }

    private async Task SaveNewPrescription(CompletePrescriptionDto prescriptionDto, Guid medicalRecordId)
    {
        var prescription = _mapper.Map<Prescription>(prescriptionDto);
        prescription.MedicalRecordId = medicalRecordId;
        prescription.PrescriptionId = Guid.NewGuid();
        await _unitOfWork.Prescriptions.Add(prescription);
        await _unitOfWork.SaveChanges();
    }

    private async Task<int> SaveNewPayment(CompletePaymentDto paymentDto)
    {
        var payment = _mapper.Map<Payment>(paymentDto);
        await _unitOfWork.Payments.Add(payment);
        await _unitOfWork.SaveChanges();
        return payment.PaymentID;
    }

    public async Task<Result<CompleteAppointmentDto>> Complete(CompleteAppointmentDto completeAppointmentDto)
    {
        var validations = await ValidateCompleteRequest(completeAppointmentDto);
        if (!validations.IsValid)
            return Result<CompleteAppointmentDto>.Failure(validations.ErrorMessage, ServiceErrorType.ValidationError);

        var appointment = await _unitOfWork.Appointments.GetById(completeAppointmentDto.AppointmentID);
        try
        {
            await _unitOfWork.CreateTransaction();
            if (appointment.MedicalRecordId.HasValue)
                await SaveNewPrescription(completeAppointmentDto.CompletePrescriptionDto, appointment.MedicalRecordId.Value);
            var paymentId = await SaveNewPayment(completeAppointmentDto.CompletePaymentDto);
            appointment.AppointmentStatus = (int)AppointmentStatus.Completed;
            appointment.PaymentID = paymentId;
            _unitOfWork.Appointments.Update(appointment);
            var result = await _unitOfWork.SaveChanges();
            await _unitOfWork.Commit();

            try
            {
                var paymentAmount = 0m;
                if (appointment.PaymentID.HasValue)
                {
                    var payment = await _context.Payments.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PaymentID == appointment.PaymentID.Value);
                    if (payment != null)
                        paymentAmount = (decimal)payment.AmountPaid;
                }
                else
                {
                    var doctor = await _context.Doctors.AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == appointment.DoctorID);
                    paymentAmount = doctor?.Price ?? 0m;
                }
                await _dashboardDoctorService.UpsertDailyEarningAsync(appointment.DoctorID, DateTime.UtcNow, paymentAmount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upsert daily earning for appointment {AppointmentId}", completeAppointmentDto.AppointmentID);
            }

            return Result<CompleteAppointmentDto>.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to complete appointment {AppointmentId}",
                completeAppointmentDto.AppointmentID, ConsoleColor.DarkRed);
            await _unitOfWork.Rollback();
            return Result<CompleteAppointmentDto>.Failure("Failed to complete the appointment",
                ServiceErrorType.DatabaseError);
        }
    }

    public async Task<Result<CreateAppointmentResponseDto>> CompleteByAppointmentId(Guid appointmentId)
    {
        var appointment = await _unitOfWork.Appointments.GetById(appointmentId);
        if (appointment is null)
            return Result<CreateAppointmentResponseDto>.Failure("Appointment not found", ServiceErrorType.NotFound);

        if (appointment.AppointmentStatus == (int)AppointmentStatus.Completed)
            return Result<CreateAppointmentResponseDto>.Failure("Appointment is already completed", ServiceErrorType.ValidationError);
        if (appointment.AppointmentStatus == (int)AppointmentStatus.Canceled)
            return Result<CreateAppointmentResponseDto>.Failure("Cannot complete a canceled appointment", ServiceErrorType.ValidationError);

        appointment.AppointmentStatus = (int)AppointmentStatus.Completed;
        _unitOfWork.Appointments.Update(appointment);
        var saved = await _unitOfWork.SaveChanges();
        if (!saved)
            return Result<CreateAppointmentResponseDto>.Failure("Failed to update appointment", ServiceErrorType.DatabaseError);

        try
        {
            var paymentAmount = 0m;
            if (appointment.PaymentID.HasValue)
            {
                var payment = await _context.Payments.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PaymentID == appointment.PaymentID.Value);
                if (payment != null)
                    paymentAmount = (decimal)payment.AmountPaid;
            }
            else
            {
                var doctor = await _context.Doctors.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == appointment.DoctorID);
                paymentAmount = doctor?.Price ?? 0m;
            }
            await _dashboardDoctorService.UpsertDailyEarningAsync(appointment.DoctorID, DateTime.UtcNow, paymentAmount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to upsert daily earning for appointment {AppointmentId}", appointmentId);
        }

        var response = await BuildCreateAppointmentResponseAsync(appointment);
        await SendAppointmentSuccessEmailsAsync(
            appointment.PatientID,
            appointment.DoctorID,
            response.PatientName,
            response.DoctorName,
            ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime),
            isReschedule: false);
        return Result<CreateAppointmentResponseDto>.Success(response);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetAll()
    {
        var appointments = await _unitOfWork.Appointments.GetAll();

        if (appointments is null)
            return Result<IEnumerable<AppointmentDto>>.Failure("There were no appointments found",
                ServiceErrorType.NotFound);

        var appointmentsDtos = _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        
        foreach (var item in appointmentsDtos)
        {
            item.StatusName = GetStatusName(item.AppointmentStatus);
            if (item.MedicalRecordId.HasValue)
            {
                var medicalRecord = await _unitOfWork.MedicalRecords.GetById(item.MedicalRecordId.Value);
                if (medicalRecord != null)
                    item.MedicalRecordDto = _mapper.Map<MedicalRecordDto>(medicalRecord);
            }
        }
        return Result<IEnumerable<AppointmentDto>>.Success(appointmentsDtos);
    }

    public async Task<Result<AppointmentDto>> GetById(Guid id)
    {
        var appointment = await _unitOfWork.Appointments.GetById(id);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("Appointment not found", ServiceErrorType.NotFound);
    
        var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
        appointmentDto.StatusName = GetStatusName(appointmentDto.AppointmentStatus);
        if (appointmentDto.MedicalRecordId.HasValue)
        {
            var medicalRecord = await _unitOfWork.MedicalRecords.GetById(appointmentDto.MedicalRecordId.Value);
            if (medicalRecord is not null)
                appointmentDto.MedicalRecordDto = _mapper.Map<MedicalRecordDto>(medicalRecord);
        }
        
        return Result<AppointmentDto>.Success(appointmentDto);
    }

    private static string FormatAppointmentDateTime(DateOnly date, TimeSpan time)
        => AppointmentScheduling.FormatDisplay(date, time);

    private string GetStatusName(short status)
    {
        return status switch
        {
            (short)AppointmentStatus.Pending => "Pending",
            (short)AppointmentStatus.Rescheduled => "Rescheduled",
            (short)AppointmentStatus.Canceled => "Canceled",
            (short)AppointmentStatus.Completed => "Completed",
            _ => "Unknown"
        };
    }

    private static AppointmentListCardDto MapToListCard(Appointment a, string formatted, string statusName)
    {
        return new AppointmentListCardDto
        {
            AppointmentID = a.AppointmentID,
            DoctorID = a.DoctorID,
            DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
            DoctorSpecialization = a.Doctor?.Specialization ?? "",
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime,
            FormattedDateTime = formatted,
            AppointmentStatus = a.AppointmentStatus,
            StatusName = statusName,
            PatientID = a.PatientID
        };
    }

    private AppointmentWithDetailsDto MapToAppointmentWithDetails(Appointment appointment)
    {
        return new AppointmentWithDetailsDto
        {
            AppointmentID = appointment.AppointmentID,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            FormattedDateTime = FormatAppointmentDateTime(appointment.AppointmentDate, appointment.AppointmentTime),
            AppointmentStatus = appointment.AppointmentStatus,
            StatusName = GetStatusName(appointment.AppointmentStatus),
            PatientID = appointment.PatientID,
            PatientName = appointment.Patient?.User?.FullName ?? "Unknown",
            PatientEmail = appointment.Patient?.User?.Email,
            PatientProfileImageUrl = appointment.Patient?.User?.ProfileImageUrl,
            DoctorID = appointment.DoctorID,
            DoctorName = appointment.Doctor?.User?.FullName ?? "Unknown",
            DoctorSpecialization = appointment.Doctor?.Specialization ?? "",
            DoctorEmail = appointment.Doctor?.User?.Email,
            DoctorProfileImageUrl = appointment.Doctor?.User?.ProfileImageUrl,
            MedicalRecordId = appointment.MedicalRecordId,
            PaymentID = appointment.PaymentID,
            AmountPaid = appointment.Payment != null ? (decimal?)appointment.Payment.AmountPaid : null,
            PaymentDate = appointment.Payment?.PaymentDate
        };
    }

    public async Task<Result<PaginatedResult<CreateAppointmentResponseDto>>> GetAppointmentsAsCreateResponse(string? patientId = null, string? doctorId = null, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Appointments.AsNoTracking();
        if (!string.IsNullOrEmpty(patientId))
            query = query.Where(a => a.PatientID == patientId);
        if (!string.IsNullOrEmpty(doctorId))
            query = query.Where(a => a.DoctorID == doctorId);

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
            return Result<PaginatedResult<CreateAppointmentResponseDto>>.Failure("No appointments found", ServiceErrorType.NotFound);

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var list = new List<CreateAppointmentResponseDto>();
        foreach (var a in appointments)
            list.Add(await BuildCreateAppointmentResponseAsync(a));

        var paged = PaginatedResult<CreateAppointmentResponseDto>.Create(list, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<CreateAppointmentResponseDto>>.Success(paged);
    }

    public async Task<Result<CreateAppointmentResponseDto>> GetByIdAsCreateResponse(Guid id)
    {
        var appointment = await _unitOfWork.Appointments.GetById(id);
        if (appointment is null)
            return Result<CreateAppointmentResponseDto>.Failure("Appointment not found", ServiceErrorType.NotFound);
        var response = await BuildCreateAppointmentResponseAsync(appointment);
        return Result<CreateAppointmentResponseDto>.Success(response);
    }

    public async Task<Result<IEnumerable<AppointmentListCardDto>>> GetAllListCards(string? patientId = null, string? doctorId = null)
    {
        var query = _context.Appointments
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(patientId))
            query = query.Where(a => a.PatientID == patientId);
        if (!string.IsNullOrEmpty(doctorId))
            query = query.Where(a => a.DoctorID == doctorId);

        var appointments = await query.ToListAsync();
        if (!appointments.Any())
            return Result<IEnumerable<AppointmentListCardDto>>.Failure("No appointments found", ServiceErrorType.NotFound);

        var list = appointments
            .Select(a => MapToListCard(a, FormatAppointmentDateTime(a.AppointmentDate, a.AppointmentTime), GetStatusName(a.AppointmentStatus)))
            .ToList();
        return Result<IEnumerable<AppointmentListCardDto>>.Success(list);
    }

    public async Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetAllWithDetails()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.MedicalRecord)
            .Include(a => a.Payment)
            .AsNoTracking()
            .ToListAsync();

        if (!appointments.Any())
            return Result<IEnumerable<AppointmentWithDetailsDto>>.Failure("No appointments found", ServiceErrorType.NotFound);

        var appointmentsWithDetails = appointments.Select(MapToAppointmentWithDetails).ToList();
        return Result<IEnumerable<AppointmentWithDetailsDto>>.Success(appointmentsWithDetails);
    }

    public async Task<Result<AppointmentWithDetailsDto>> GetByIdWithDetails(Guid id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.MedicalRecord)
            .Include(a => a.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AppointmentID == id);

        if (appointment is null)
            return Result<AppointmentWithDetailsDto>.Failure("Appointment not found", ServiceErrorType.NotFound);

        var appointmentWithDetails = MapToAppointmentWithDetails(appointment);
        return Result<AppointmentWithDetailsDto>.Success(appointmentWithDetails);
    }

    public async Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetByPatientIdWithDetails(string patientId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.MedicalRecord)
            .Include(a => a.Payment)
            .Where(a => a.PatientID == patientId)
            .AsNoTracking()
            .ToListAsync();

        if (!appointments.Any())
            return Result<IEnumerable<AppointmentWithDetailsDto>>.Failure("No appointments found for this patient", ServiceErrorType.NotFound);

        var appointmentsWithDetails = appointments.Select(MapToAppointmentWithDetails).ToList();
        return Result<IEnumerable<AppointmentWithDetailsDto>>.Success(appointmentsWithDetails);
    }

    public async Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetByDoctorIdWithDetails(string doctorId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.MedicalRecord)
            .Include(a => a.Payment)
            .Where(a => a.DoctorID == doctorId)
            .AsNoTracking()
            .ToListAsync();

        if (!appointments.Any())
            return Result<IEnumerable<AppointmentWithDetailsDto>>.Failure("No appointments found for this doctor", ServiceErrorType.NotFound);

        var appointmentsWithDetails = appointments.Select(MapToAppointmentWithDetails).ToList();
        return Result<IEnumerable<AppointmentWithDetailsDto>>.Success(appointmentsWithDetails);
    }

    public async Task<Result<IEnumerable<AppointmentWithDetailsDto>>> GetUpcomingAppointments(string? doctorId = null, string? patientId = null)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;
        var query = _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.MedicalRecord)
            .Include(a => a.Payment)
            .Where(a => a.AppointmentStatus != (short)AppointmentStatus.Canceled
                        && (a.AppointmentDate > today
                            || (a.AppointmentDate == today && a.AppointmentTime >= nowTime)));

        if (doctorId != null)
            query = query.Where(a => a.DoctorID == doctorId);

        if (patientId != null)
            query = query.Where(a => a.PatientID == patientId);

        var upcoming = await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        if (!upcoming.Any())
            return Result<IEnumerable<AppointmentWithDetailsDto>>.Failure("No upcoming appointments found", ServiceErrorType.NotFound);

        var appointmentsWithDetails = upcoming.Select(MapToAppointmentWithDetails).ToList();
        return Result<IEnumerable<AppointmentWithDetailsDto>>.Success(appointmentsWithDetails);
    }

    public async Task<Result<CreateAppointmentResponseDto>> SubmitRating(SubmitDoctorRatingDto dto, string patientId)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            return Result<CreateAppointmentResponseDto>.Failure("Rating must be between 1 and 5.", ServiceErrorType.ValidationError);

        var appointment = await _unitOfWork.Appointments.GetById(dto.AppointmentID);
        if (appointment == null)
            return Result<CreateAppointmentResponseDto>.Failure("Appointment not found.", ServiceErrorType.NotFound);
        if (appointment.AppointmentStatus != (short)AppointmentStatus.Completed)
            return Result<CreateAppointmentResponseDto>.Failure("Only completed appointments can be rated.", ServiceErrorType.ValidationError);
        if (appointment.PatientID != patientId)
            return Result<CreateAppointmentResponseDto>.Failure("You can only rate your own appointment.", ServiceErrorType.ValidationError);

        var alreadyRated = await _unitOfWork.DoctorRatings.ExistsAsync(r => r.AppointmentID == dto.AppointmentID);
        if (alreadyRated)
            return Result<CreateAppointmentResponseDto>.Failure("You have already rated this appointment.", ServiceErrorType.ValidationError);

        var doctorRating = new DoctorRating
        {
            AppointmentID = dto.AppointmentID,
            DoctorID = appointment.DoctorID,
            PatientID = patientId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };
        await _unitOfWork.DoctorRatings.Add(doctorRating);
        await _unitOfWork.SaveChanges();

        var ratings = await _unitOfWork.DoctorRatings.FindAll(r => r.DoctorID == appointment.DoctorID);
        var list = ratings.ToList();
        var doctor = await _unitOfWork.Doctors.GetById(appointment.DoctorID);
        if (doctor != null && list.Any())
        {
            doctor.Rating = (decimal)list.Average(r => r.Rating);
            doctor.RatingCount = list.Count;
            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChanges();
        }

        var response = await BuildCreateAppointmentResponseAsync(appointment);
        await SendAppointmentSuccessEmailsAsync(
            appointment.PatientID,
            appointment.DoctorID,
            response.PatientName,
            response.DoctorName,
            ToUtcDateTime(appointment.AppointmentDate, appointment.AppointmentTime),
            isReschedule: false);
        return Result<CreateAppointmentResponseDto>.Success(response);
    }
}
