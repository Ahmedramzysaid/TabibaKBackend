using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DataAccessLayer.Persistence;

namespace BusinessLayer.Services;

public class DashboardDoctorService : IDashboardDoctorService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public DashboardDoctorService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }


    public async Task<Result<DoctorDashboardOverviewDto>> GetOverviewAsync(string doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor == null)
            return Result<DoctorDashboardOverviewDto>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;

        var baseQuery = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId);

        var todayQuery = baseQuery.Where(a => a.AppointmentDate == today && a.AppointmentStatus != (short)AppointmentStatus.Canceled);
        var todayCount = await todayQuery.CountAsync();
        var completedTodayCount = await todayQuery.CountAsync(a => a.AppointmentStatus == (short)AppointmentStatus.Completed);
        var pendingTodayCount = await todayQuery.CountAsync(a => a.AppointmentStatus == (short)AppointmentStatus.Pending);

        var activePatientsCount = await baseQuery
            .Where(a => a.AppointmentStatus != (short)AppointmentStatus.Canceled)
            .Select(a => a.PatientID)
            .Distinct()
            .CountAsync();

        var completedMedicalRecordIds = await baseQuery
            .Where(a => a.AppointmentStatus == (short)AppointmentStatus.Completed && a.MedicalRecordId != null)
            .Select(a => a.MedicalRecordId!.Value)
            .Distinct()
            .ToListAsync();

        var pendingReportsCount = 0;
        if (completedMedicalRecordIds.Any())
        {
            pendingReportsCount = await _context.Prescriptions
                .AsNoTracking()
                .CountAsync(p => completedMedicalRecordIds.Contains(p.MedicalRecordId) && p.Status == PrescriptionStatus.StillUnderDoctor);
        }

        var totalEarnings = await baseQuery
            .Where(a => a.AppointmentStatus == (short)AppointmentStatus.Completed && a.PaymentID != null)
            .Join(_context.Payments, a => a.PaymentID, p => p.PaymentID, (a, p) => (decimal)p.AmountPaid)
            .SumAsync(amount => amount);

        var todayEarnings = await todayQuery
            .Where(a => a.AppointmentStatus == (short)AppointmentStatus.Completed && a.PaymentID != null)
            .Join(_context.Payments, a => a.PaymentID, p => p.PaymentID, (a, p) => (decimal)p.AmountPaid)
            .SumAsync(amount => amount);

        var nextAppointment = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                        && a.AppointmentStatus != (short)AppointmentStatus.Canceled
                        && (a.AppointmentDate > today
                            || (a.AppointmentDate == today && a.AppointmentTime >= nowTime)))
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .FirstOrDefaultAsync();

        DoctorNextAppointmentDto? nextDto = null;
        if (nextAppointment != null)
        {
            nextDto = new DoctorNextAppointmentDto
            {
                AppointmentID = nextAppointment.AppointmentID,
                PatientName = nextAppointment.Patient?.User?.FullName ?? "Unknown",
                PatientAge = nextAppointment.Patient?.User?.DateOfBirth != null ? (int?)((DateTime.UtcNow - nextAppointment.Patient.User.DateOfBirth).Days / 365) : null,
                AppointmentType = "Consultation",
                AppointmentDate = nextAppointment.AppointmentDate,
                AppointmentTime = nextAppointment.AppointmentTime,
                FormattedDateTime = FormatDateTime(nextAppointment.AppointmentDate, nextAppointment.AppointmentTime),
                AppointmentStatus = nextAppointment.AppointmentStatus,
                StatusName = GetStatusName(nextAppointment.AppointmentStatus)
            };
        }

        var overview = new DoctorDashboardOverviewDto
        {
            DoctorId = doctor.Id,
            FullName = doctor.User?.FullName ?? "Doctor",
            Specialization = doctor.Specialization,
            IsAvailable = doctor.IsAvailable,
            ProfileImageUrl = doctor.User?.ProfileImageUrl,
            TodayAppointmentsCount = todayCount,
            CompletedTodayCount = completedTodayCount,
            PendingTodayCount = pendingTodayCount,
            ActivePatientsCount = activePatientsCount,
            PendingReportsCount = pendingReportsCount,
            TotalEarningsEgp = totalEarnings,
            TodayEarningsEgp = todayEarnings,
            Rating = doctor.Rating,
            RatingCount = doctor.RatingCount,
            NextAppointment = nextDto
        };

        return Result<DoctorDashboardOverviewDto>.Success(overview);
    }


    public async Task<Result<IEnumerable<DoctorTodayAppointmentItemDto>>> GetTodayAppointmentsAsync(string doctorId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId && a.AppointmentDate == today && a.AppointmentStatus != (short)AppointmentStatus.Canceled)
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new DoctorTodayAppointmentItemDto
            {
                AppointmentID = a.AppointmentID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                TimeAndPurpose = "",
                StatusName = "",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                AppointmentStatus = a.AppointmentStatus
            })
            .ToListAsync();

        foreach (var item in items)
        {
            item.TimeAndPurpose = $"{AppointmentScheduling.FormatTime12Hour(item.AppointmentTime)} - Consultation";
            item.StatusName = GetStatusName(item.AppointmentStatus);
        }

        return Result<IEnumerable<DoctorTodayAppointmentItemDto>>.Success(items);
    }


    public async Task<Result<DoctorNextAppointmentDto?>> GetNextAppointmentAsync(string doctorId)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;

        var next = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                        && a.AppointmentStatus != (short)AppointmentStatus.Canceled
                        && (a.AppointmentDate > today
                            || (a.AppointmentDate == today && a.AppointmentTime >= nowTime)))
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .FirstOrDefaultAsync();

        if (next == null)
            return Result<DoctorNextAppointmentDto?>.Success(null);

        var dto = new DoctorNextAppointmentDto
        {
            AppointmentID = next.AppointmentID,
            PatientName = next.Patient?.User?.FullName ?? "Unknown",
            PatientAge = next.Patient?.User?.DateOfBirth != null ? (int?)((DateTime.UtcNow - next.Patient.User.DateOfBirth).Days / 365) : null,
            AppointmentType = "Consultation",
            AppointmentDate = next.AppointmentDate,
            AppointmentTime = next.AppointmentTime,
            FormattedDateTime = FormatDateTime(next.AppointmentDate, next.AppointmentTime),
            AppointmentStatus = next.AppointmentStatus,
            StatusName = GetStatusName(next.AppointmentStatus)
        };

        return Result<DoctorNextAppointmentDto?>.Success(dto);
    }


    public async Task<Result<IEnumerable<DoctorRecentPatientDto>>> GetRecentPatientsAsync(string doctorId, int count = 10)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;

        var recentAppointments = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId && a.AppointmentStatus != (short)AppointmentStatus.Canceled)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();

        var seenPatients = new HashSet<string>();
        var uniqueRecentAppointments = new List<Appointment>();
        var patientAppointmentCounts = recentAppointments
            .GroupBy(a => a.PatientID)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var a in recentAppointments)
        {
            if (seenPatients.Contains(a.PatientID)) continue;
            seenPatients.Add(a.PatientID);
            uniqueRecentAppointments.Add(a);
            if (uniqueRecentAppointments.Count >= count) break;
        }

        var uniquePatientIds = uniqueRecentAppointments.Select(a => a.PatientID).ToList();
        var futureAppointments = await _context.Appointments
            .AsNoTracking()
            .Where(ap => ap.DoctorID == doctorId
                      && uniquePatientIds.Contains(ap.PatientID)
                      && ap.AppointmentStatus != (short)AppointmentStatus.Canceled
                      && (ap.AppointmentDate > today
                          || (ap.AppointmentDate == today && ap.AppointmentTime >= nowTime)))
            .Select(ap => new { ap.PatientID, ap.AppointmentDate, ap.AppointmentTime })
            .ToListAsync();

        var nextAppointmentsForPatients = futureAppointments
            .GroupBy(ap => ap.PatientID)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.AppointmentDate).ThenBy(x => x.AppointmentTime).First());

        var recentPatients = uniqueRecentAppointments.Select(a =>
        {
            var isNewPatient = patientAppointmentCounts.GetValueOrDefault(a.PatientID, 0) <= 1;
            nextAppointmentsForPatients.TryGetValue(a.PatientID, out var next);

            return new DoctorRecentPatientDto
            {
                PatientID = a.PatientID,
                PatientName = a.Patient?.User?.FullName ?? "Unknown",
                PatientAge = a.Patient?.User?.DateOfBirth != null ? (int?)((DateTime.UtcNow - a.Patient.User.DateOfBirth).Days / 365) : null,
                PatientType = isNewPatient ? "New Patient" : "Follow-up",
                NextAppointmentFormatted = next != null
                    ? FormatDateTime(next.AppointmentDate, next.AppointmentTime)
                    : null,
                NextAppointmentDate = next?.AppointmentDate,
                NextAppointmentTime = next?.AppointmentTime
            };
        }).ToList();

        return Result<IEnumerable<DoctorRecentPatientDto>>.Success(recentPatients);
    }


    public async Task<Result<bool>> SetAvailabilityAsync(string doctorId, bool isAvailable)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
        if (doctor == null)
            return Result<bool>.Failure("Doctor not found", ServiceErrorType.NotFound);

        doctor.IsAvailable = isAvailable;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(isAvailable);
    }


    public async Task<Result<IEnumerable<DoctorScheduledAppointmentDto>>> GetScheduledAppointmentsAsync(string doctorId)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<IEnumerable<DoctorScheduledAppointmentDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                     && a.AppointmentDate >= today
                     && a.AppointmentStatus != (short)AppointmentStatus.Canceled)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .Select(a => new DoctorScheduledAppointmentDto
            {
                AppointmentID = a.AppointmentID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                PatientProfileImageUrl = a.Patient.User.ProfileImageUrl,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                FormattedDateTime = "",
                AppointmentStatus = a.AppointmentStatus,
                StatusName = ""
            })
            .ToListAsync();

        foreach (var item in items)
        {
            item.FormattedDateTime = FormatDateTime(item.AppointmentDate, item.AppointmentTime);
            item.StatusName = GetStatusName(item.AppointmentStatus);
        }

        return Result<IEnumerable<DoctorScheduledAppointmentDto>>.Success(items);
    }


    public async Task<Result<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetPastMedicalRecordsAsync(string doctorId)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<IEnumerable<DoctorMedicalRecordSummaryDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;

        var records = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                        && a.MedicalRecordId != null
                        && (a.AppointmentDate < today
                            || (a.AppointmentDate == today && a.AppointmentTime < nowTime)))
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .Select(a => new DoctorMedicalRecordSummaryDto
            {
                MedicalRecordId = a.MedicalRecordId!.Value,
                PatientId = a.PatientID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                FormattedDateTime = "",
                AppointmentStatus = a.AppointmentStatus,
                StatusName = ""
            })
            .ToListAsync();

        foreach (var r in records)
        {
            r.FormattedDateTime = FormatDateTime(r.AppointmentDate, r.AppointmentTime);
            r.StatusName = GetStatusName(r.AppointmentStatus);
        }

        return Result<IEnumerable<DoctorMedicalRecordSummaryDto>>.Success(records);
    }

    public async Task<Result<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetFutureMedicalRecordsAsync(string doctorId)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<IEnumerable<DoctorMedicalRecordSummaryDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = now.TimeOfDay;

        var records = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                        && a.MedicalRecordId != null
                        && (a.AppointmentDate > today
                            || (a.AppointmentDate == today && a.AppointmentTime >= nowTime)))
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .Select(a => new DoctorMedicalRecordSummaryDto
            {
                MedicalRecordId = a.MedicalRecordId!.Value,
                PatientId = a.PatientID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                FormattedDateTime = "",
                AppointmentStatus = a.AppointmentStatus,
                StatusName = ""
            })
            .ToListAsync();

        foreach (var r in records)
        {
            r.FormattedDateTime = FormatDateTime(r.AppointmentDate, r.AppointmentTime);
            r.StatusName = GetStatusName(r.AppointmentStatus);
        }

        return Result<IEnumerable<DoctorMedicalRecordSummaryDto>>.Success(records);
    }


    public async Task<Result<bool>> SetPriceAsync(string doctorId, decimal price)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
        if (doctor == null)
            return Result<bool>.Failure("Doctor not found", ServiceErrorType.NotFound);

        doctor.Price = price;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }


    public async Task<Result<IEnumerable<DoctorScheduleResponseDto>>> SetScheduleAsync(string doctorId, List<SetDoctorScheduleDto> schedules)
    {
        var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == doctorId);
        if (doctor == null)
            return Result<IEnumerable<DoctorScheduleResponseDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        foreach (var s in schedules)
        {
            if (s.StartTime >= s.EndTime)
                return Result<IEnumerable<DoctorScheduleResponseDto>>.Failure(
                    $"Start time must be before end time for {s.DayOfWeek}.",
                    ServiceErrorType.ValidationError);
            if (s.SlotDurationMinutes < 5 || s.SlotDurationMinutes > 240)
                return Result<IEnumerable<DoctorScheduleResponseDto>>.Failure(
                    $"Slot duration must be between 5 and 240 minutes for {s.DayOfWeek}.",
                    ServiceErrorType.ValidationError);
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var existing = await _context.DoctorSchedules.Where(s => s.DoctorId == doctorId).ToListAsync();
            _context.DoctorSchedules.RemoveRange(existing);

            foreach (var s in schedules)
            {
                _context.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctorId,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SlotDurationMinutes = s.SlotDurationMinutes,
                    IsActive = s.IsActive
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        });

        _cache.Remove($"doctor-schedule-{doctorId}");

        return await GetScheduleAsync(doctorId);
    }

    public async Task<Result<IEnumerable<DoctorScheduleResponseDto>>> GetScheduleAsync(string doctorId)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<IEnumerable<DoctorScheduleResponseDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var schedules = await _context.DoctorSchedules
            .AsNoTracking()
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.DayOfWeek)
            .Select(s => new DoctorScheduleResponseDto
            {
                Id = s.Id,
                DayName = s.DayOfWeek.ToString(),
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDurationMinutes = s.SlotDurationMinutes,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Result<IEnumerable<DoctorScheduleResponseDto>>.Success(schedules);
    }


    public async Task<Result<PaginatedResult<DoctorTodayAppointmentItemDto>>> GetNotCompletedAppointmentsAsync(
        string doctorId, int page = 1, int pageSize = 20)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<PaginatedResult<DoctorTodayAppointmentItemDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                     && a.AppointmentStatus != (short)AppointmentStatus.Completed
                     && a.AppointmentStatus != (short)AppointmentStatus.Canceled);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new DoctorTodayAppointmentItemDto
            {
                AppointmentID = a.AppointmentID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                TimeAndPurpose = "",
                StatusName = "",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                AppointmentStatus = a.AppointmentStatus
            })
            .ToListAsync();

        foreach (var item in items)
        {
            item.TimeAndPurpose = $"{AppointmentScheduling.FormatTime12Hour(item.AppointmentTime)} - Consultation";
            item.StatusName = GetStatusName(item.AppointmentStatus);
        }

        var paginated = PaginatedResult<DoctorTodayAppointmentItemDto>.Create(items, totalCount, page, pageSize);
        return Result<PaginatedResult<DoctorTodayAppointmentItemDto>>.Success(paginated);
    }


    public async Task<Result<PaginatedResult<DoctorTodayAppointmentItemDto>>> GetCompletedAppointmentsAsync(
        string doctorId, int page = 1, int pageSize = 20)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<PaginatedResult<DoctorTodayAppointmentItemDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                     && a.AppointmentStatus == (short)AppointmentStatus.Completed);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new DoctorTodayAppointmentItemDto
            {
                AppointmentID = a.AppointmentID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                TimeAndPurpose = "",
                StatusName = "",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                AppointmentStatus = a.AppointmentStatus
            })
            .ToListAsync();

        foreach (var item in items)
        {
            item.TimeAndPurpose = $"{AppointmentScheduling.FormatTime12Hour(item.AppointmentTime)} - Consultation";
            item.StatusName = GetStatusName(item.AppointmentStatus);
        }

        var paginated = PaginatedResult<DoctorTodayAppointmentItemDto>.Create(items, totalCount, page, pageSize);
        return Result<PaginatedResult<DoctorTodayAppointmentItemDto>>.Success(paginated);
    }


    public async Task<Result<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetMedicalRecordsByPatientAsync(
        string doctorId, string patientId, bool completedOnly)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<IEnumerable<DoctorMedicalRecordSummaryDto>>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                     && a.PatientID == patientId
                     && a.MedicalRecordId != null);

        if (completedOnly)
            query = query.Where(a => a.AppointmentStatus == (short)AppointmentStatus.Completed);
        else
            query = query.Where(a => a.AppointmentStatus != (short)AppointmentStatus.Completed
                                  && a.AppointmentStatus != (short)AppointmentStatus.Canceled);

        var records = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .Select(a => new DoctorMedicalRecordSummaryDto
            {
                MedicalRecordId = a.MedicalRecordId!.Value,
                PatientId = a.PatientID,
                PatientName = a.Patient.User.FullName ?? "Unknown",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                FormattedDateTime = "",
                AppointmentStatus = a.AppointmentStatus,
                StatusName = ""
            })
            .ToListAsync();

        foreach (var r in records)
        {
            r.FormattedDateTime = FormatDateTime(r.AppointmentDate, r.AppointmentTime);
            r.StatusName = GetStatusName(r.AppointmentStatus);
        }

        return Result<IEnumerable<DoctorMedicalRecordSummaryDto>>.Success(records);
    }


    public async Task<Result<DoctorTodayEarningsDto>> GetTodayEarningsAsync(string doctorId)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<DoctorTodayEarningsDto>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todayQuery = _context.Appointments
            .Include(a => a.Payment)
            .AsNoTracking()
            .Where(a => a.DoctorID == doctorId
                     && a.AppointmentDate == today
                     && a.AppointmentStatus == (short)AppointmentStatus.Completed
                     && a.PaymentID != null);

        var completedCount = await todayQuery.CountAsync();
        var totalEarnings = await todayQuery
            .Where(a => a.Payment != null)
            .SumAsync(a => (decimal)a.Payment!.AmountPaid);

        var dto = new DoctorTodayEarningsDto
        {
            Date = today.ToDateTime(TimeOnly.MinValue),
            TotalEarnings = totalEarnings,
            CompletedAppointmentsCount = completedCount
        };

        return Result<DoctorTodayEarningsDto>.Success(dto);
    }


    public async Task<Result<DoctorFinancialReportDto>> GetFinancialReportAsync(string doctorId, DateTime from, DateTime to)
    {
        var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<DoctorFinancialReportDto>.Failure("Doctor not found", ServiceErrorType.NotFound);

        if (from > to)
            return Result<DoctorFinancialReportDto>.Failure("'From' date must be before or equal to 'To' date.", ServiceErrorType.ValidationError);

        from = from.Date;
        to = to.Date;

        var dailyEarnings = await _context.DoctorEarnings
            .AsNoTracking()
            .Where(e => e.DoctorId == doctorId && e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date)
            .Select(e => new DoctorDailyEarningDto
            {
                Date = e.Date,
                Earnings = e.DailyEarnings,
                AppointmentCount = e.AppointmentCount
            })
            .ToListAsync();

        var n = dailyEarnings.Count;
        var prefixSums = new List<decimal>(n + 1) { 0m };
        for (int i = 0; i < n; i++)
        {
            prefixSums.Add(prefixSums[i] + dailyEarnings[i].Earnings);
        }

        var totalEarnings = prefixSums[n]; // prefix[n] = total sum
        var totalAppointments = dailyEarnings.Sum(d => d.AppointmentCount);

        var report = new DoctorFinancialReportDto
        {
            From = from,
            To = to,
            TotalEarnings = totalEarnings,
            TotalAppointments = totalAppointments,
            DailyBreakdown = dailyEarnings,
            PrefixSums = prefixSums
        };

        return Result<DoctorFinancialReportDto>.Success(report);
    }


    public async Task UpsertDailyEarningAsync(string doctorId, DateTime date, decimal amount)
    {
        var dateOnly = date.Date;

        var existing = await _context.DoctorEarnings
            .FirstOrDefaultAsync(e => e.DoctorId == doctorId && e.Date == dateOnly);

        if (existing != null)
        {
            existing.DailyEarnings += amount;
            existing.AppointmentCount += 1;
        }
        else
        {
            _context.DoctorEarnings.Add(new DoctorEarning
            {
                DoctorId = doctorId,
                Date = dateOnly,
                DailyEarnings = amount,
                AppointmentCount = 1
            });
        }

        await _context.SaveChangesAsync();
    }


    private static string FormatDateTime(DateOnly date, TimeSpan time)
        => AppointmentScheduling.FormatDisplay(date, time);

    private static string GetStatusName(short status)
    {
        return status switch
        {
            (short)AppointmentStatus.Pending => "Pending",
            (short)AppointmentStatus.Rescheduled => "Rescheduled",
            (short)AppointmentStatus.Completed => "Completed",
            (short)AppointmentStatus.Canceled => "Canceled",
            _ => "Unknown"
        };
    }
}
