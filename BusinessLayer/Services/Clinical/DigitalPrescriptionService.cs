using AutoMapper;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Persistence;

namespace BusinessLayer.Services;

public class DigitalPrescriptionService : IDigitalPrescriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DigitalPrescriptionService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<DigitalPrescriptionDto>>> GetAll(string? doctorId = null, string? patientId = null, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(doctorId))
            query = query.Where(d => _context.Appointments.Any(a => a.DoctorID == doctorId && a.PatientID == d.MedicalRecord.PatientId));
        if (!string.IsNullOrEmpty(patientId))
            query = query.Where(d => d.MedicalRecord.PatientId == patientId);

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
            return Result<PaginatedResult<DigitalPrescriptionDto>>.Success(
                PaginatedResult<DigitalPrescriptionDto>.Create(Enumerable.Empty<DigitalPrescriptionDto>(), 0, pageNumber, pageSize));

        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<IEnumerable<DigitalPrescriptionDto>>(list);
        var paged = PaginatedResult<DigitalPrescriptionDto>.Create(dtos, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<DigitalPrescriptionDto>>.Success(paged);
    }

    public async Task<Result<DigitalPrescriptionDto>> GetById(Guid id, string? doctorId = null, string? patientId = null)
    {
        var dp = await _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DigitalPrescriptionId == id);
        if (dp == null)
            return Result<DigitalPrescriptionDto>.Failure("Digital prescription not found", ServiceErrorType.NotFound);
        if (!string.IsNullOrEmpty(doctorId) && !await _context.Appointments.AnyAsync(a => a.DoctorID == doctorId && a.PatientID == dp.MedicalRecord.PatientId))
            return Result<DigitalPrescriptionDto>.Failure("Digital prescription not found", ServiceErrorType.NotFound);
        if (!string.IsNullOrEmpty(patientId) && dp.MedicalRecord.PatientId != patientId)
            return Result<DigitalPrescriptionDto>.Failure("Digital prescription not found", ServiceErrorType.NotFound);
        var dto = _mapper.Map<DigitalPrescriptionDto>(dp);
        return Result<DigitalPrescriptionDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<DigitalPrescriptionDto>>> GetByMedicalRecordId(Guid medicalRecordId, string? patientId = null)
    {
        var query = _context.DigitalPrescriptions
            .Include(d => d.Items)
            .AsNoTracking()
            .Where(d => d.MedicalRecordId == medicalRecordId);
        if (!string.IsNullOrEmpty(patientId))
        {
            var mr = await _context.MedicalRecords.AsNoTracking().FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);
            if (mr?.PatientId != patientId)
                return Result<IEnumerable<DigitalPrescriptionDto>>.Failure("Medical record not found", ServiceErrorType.NotFound);
        }
        var list = await query.ToListAsync();
        var dtos = _mapper.Map<IEnumerable<DigitalPrescriptionDto>>(list);
        return Result<IEnumerable<DigitalPrescriptionDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<DigitalPrescriptionDto>>> GetByPatientId(string patientId, string? doctorId = null)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return Result<IEnumerable<DigitalPrescriptionDto>>.Failure("Patient ID is required", ServiceErrorType.ValidationError);

        var query = _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .AsNoTracking()
            .Where(d => d.MedicalRecord.PatientId == patientId);

        if (!string.IsNullOrEmpty(doctorId))
            query = query.Where(d => _context.Appointments.Any(a => a.DoctorID == doctorId && a.PatientID == d.MedicalRecord.PatientId));

        var list = await query.ToListAsync();
        var dtos = _mapper.Map<IEnumerable<DigitalPrescriptionDto>>(list);
        return Result<IEnumerable<DigitalPrescriptionDto>>.Success(dtos);
    }

    public async Task<Result<DigitalPrescriptionDto>> Create(CreateDigitalPrescriptionDto dto, string? doctorIdForWrite = null)
    {
        if (dto == null)
            return Result<DigitalPrescriptionDto>.Failure("Request is required", ServiceErrorType.ValidationError);
        var medicalRecord = await _context.MedicalRecords
            .FirstOrDefaultAsync(m => m.MedicalRecordId == dto.MedicalRecordId);
        if (medicalRecord == null)
            return Result<DigitalPrescriptionDto>.Failure("Medical record not found", ServiceErrorType.NotFound);
        if (!string.IsNullOrEmpty(doctorIdForWrite))
        {
            var hasAppointment = await _context.Appointments
                .AnyAsync(a => a.MedicalRecordId == dto.MedicalRecordId && a.DoctorID == doctorIdForWrite);
            if (!hasAppointment)
                return Result<DigitalPrescriptionDto>.Failure("You can only create digital prescriptions for medical records linked to your appointments", ServiceErrorType.ValidationError);
        }

        var entity = new DigitalPrescription
        {
            DigitalPrescriptionId = Guid.NewGuid(),
            MedicalRecordId = dto.MedicalRecordId,
            CreatedAt = DateTime.UtcNow,
            Items = new List<DigitalPrescriptionItem>()
        };
        if (dto.Items != null)
        {
            foreach (var item in dto.Items)
            {
                entity.Items.Add(new DigitalPrescriptionItem
                {
                    DigitalPrescriptionItemId = Guid.NewGuid(),
                    MedicineName = item.MedicineName,
                    PerDay = item.PerDay,
                    Spotlights = item.Spotlights
                });
            }
        }
        _context.DigitalPrescriptions.Add(entity);
        await _context.SaveChangesAsync();
        var created = await _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .AsNoTracking()
            .FirstAsync(d => d.DigitalPrescriptionId == entity.DigitalPrescriptionId);
        var resultDto = _mapper.Map<DigitalPrescriptionDto>(created);
        if (!string.IsNullOrEmpty(doctorIdForWrite))
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorIdForWrite);
            if (doctor != null)
            {
                resultDto.DoctorName = doctor.User?.FullName;
                resultDto.Specialist = doctor.Specialization;
            }
        }
        return Result<DigitalPrescriptionDto>.Success(resultDto);
    }

    public async Task<Result<DigitalPrescriptionDto>> Update(Guid id, CreateDigitalPrescriptionDto dto, string? doctorIdForWrite = null)
    {
        if (dto == null)
            return Result<DigitalPrescriptionDto>.Failure("Request is required", ServiceErrorType.ValidationError);
        var existing = await _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .FirstOrDefaultAsync(d => d.DigitalPrescriptionId == id);
        if (existing == null)
            return Result<DigitalPrescriptionDto>.Failure("Digital prescription not found", ServiceErrorType.NotFound);
        if (!string.IsNullOrEmpty(doctorIdForWrite))
        {
            var hasAppointment = await _context.Appointments
                .AnyAsync(a => a.MedicalRecordId == existing.MedicalRecordId && a.DoctorID == doctorIdForWrite);
            if (!hasAppointment)
                return Result<DigitalPrescriptionDto>.Failure("You can only update digital prescriptions for medical records linked to your appointments", ServiceErrorType.ValidationError);
        }

        existing.Items.Clear();
        if (dto.Items != null)
        {
            foreach (var item in dto.Items)
            {
                existing.Items.Add(new DigitalPrescriptionItem
                {
                    DigitalPrescriptionItemId = Guid.NewGuid(),
                    DigitalPrescriptionId = id,
                    MedicineName = item.MedicineName,
                    PerDay = item.PerDay,
                    Spotlights = item.Spotlights
                });
            }
        }
        await _context.SaveChangesAsync();
        var updated = await _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .AsNoTracking()
            .FirstAsync(d => d.DigitalPrescriptionId == id);
        var resultDto = _mapper.Map<DigitalPrescriptionDto>(updated);
        if (!string.IsNullOrEmpty(doctorIdForWrite))
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorIdForWrite);
            if (doctor != null)
            {
                resultDto.DoctorName = doctor.User?.FullName;
                resultDto.Specialist = doctor.Specialization;
            }
        }
        return Result<DigitalPrescriptionDto>.Success(resultDto);
    }

    public async Task<Result<bool>> Delete(Guid id, string? doctorIdForWrite = null)
    {
        var existing = await _context.DigitalPrescriptions
            .Include(d => d.Items)
            .Include(d => d.MedicalRecord)
            .FirstOrDefaultAsync(d => d.DigitalPrescriptionId == id);
        if (existing == null)
            return Result<bool>.Failure("Digital prescription not found", ServiceErrorType.NotFound);
        if (!string.IsNullOrEmpty(doctorIdForWrite))
        {
            var hasAppointment = await _context.Appointments
                .AnyAsync(a => a.MedicalRecordId == existing.MedicalRecordId && a.DoctorID == doctorIdForWrite);
            if (!hasAppointment)
                return Result<bool>.Failure("You can only delete digital prescriptions for medical records linked to your appointments", ServiceErrorType.ValidationError);
        }
        _context.DigitalPrescriptions.Remove(existing);
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
