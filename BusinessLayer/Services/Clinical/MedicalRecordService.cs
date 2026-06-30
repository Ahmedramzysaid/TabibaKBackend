using AutoMapper;
using BusinessLayer.Validations;
using DomainLayer.DTOs;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly DataAccessLayer.Persistence.ApplicationDbContext _context;

    public MedicalRecordService(IUnitOfWork unitOfWork, IMapper mapper, DataAccessLayer.Persistence.ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PaginatedResult<MedicalRecordDto>>> GetAll(string? doctorId = null, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.MedicalRecords.Include(m => m.Patient).AsNoTracking();

        if (!string.IsNullOrEmpty(doctorId))
        {
            var patientIdsWithPendingAppointment = await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.AppointmentStatus == (short)DomainLayer.Enums.AppointmentStatus.Pending)
                .Select(a => a.PatientID)
                .Distinct()
                .ToListAsync();
            query = query.Where(m => patientIdsWithPendingAppointment.Contains(m.PatientId));
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
            return Result<PaginatedResult<MedicalRecordDto>>.Failure("No medical records found", ServiceErrorType.NotFound);

        var medicalRecords = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var medicalRecordDtos = _mapper.Map<IEnumerable<MedicalRecordDto>>(medicalRecords);
        var paged = PaginatedResult<MedicalRecordDto>.Create(medicalRecordDtos, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<MedicalRecordDto>>.Success(paged);
    }

    public async Task<Result<MedicalRecordDto>> GetById(Guid id, string? doctorId = null)
    {
        var medicalRecord = await _context.MedicalRecords
            .Include(m => m.Patient)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MedicalRecordId == id);

        if (medicalRecord == null)
            return Result<MedicalRecordDto>.Failure("Medical record not found", ServiceErrorType.NotFound);

        if (!string.IsNullOrEmpty(doctorId))
        {
            var patientHasPendingAppointmentWithDoctor = await _context.Appointments
                .AnyAsync(a => a.DoctorID == doctorId && a.PatientID == medicalRecord.PatientId && a.AppointmentStatus == (short)DomainLayer.Enums.AppointmentStatus.Pending);
            if (!patientHasPendingAppointmentWithDoctor)
                return Result<MedicalRecordDto>.Failure("Medical record not found", ServiceErrorType.NotFound);
        }

        var medicalRecordDto = _mapper.Map<MedicalRecordDto>(medicalRecord);
        return Result<MedicalRecordDto>.Success(medicalRecordDto);
    }
    
    public async Task<Result<MedicalRecordDto>> GetByPatientId(string patientId)
    {
        var medicalRecord = await _context.MedicalRecords
            .Include(m => m.Patient)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.PatientId == patientId);
        
        if (medicalRecord == null)
            return Result<MedicalRecordDto>.Failure("No medical record found for this patient", ServiceErrorType.NotFound);
    
        var medicalRecordDto = _mapper.Map<MedicalRecordDto>(medicalRecord);
        return Result<MedicalRecordDto>.Success(medicalRecordDto);
    }
    
    
    public async Task<Result<MedicalRecordDto>> Add(CreateMedicalRecordDto createMedicalRecordDto)
    {
        return Result<MedicalRecordDto>.Failure("MedicalRecord cannot be created manually. It is automatically created when a Patient is created.", ServiceErrorType.ValidationError);
    }

    public async Task<Result<bool>> Update(MedicalRecordDto medicalRecordDto)
    {
        return Result<bool>.Failure("MedicalRecord cannot be updated. It only stores MedicalRecordId and PatientId.", ServiceErrorType.ValidationError);
    }

    public async Task<Result<bool>> Delete(Guid id)
    {
        var medicalRecord = await _unitOfWork.MedicalRecords.GetById(id);
    
        if (medicalRecord == null)
            return Result<bool>.Failure("Medical record not found", ServiceErrorType.NotFound);
    
        _unitOfWork.MedicalRecords.Delete(medicalRecord);
    
        bool saveResult = await _unitOfWork.SaveChanges();
    
        if (!saveResult)
            return Result<bool>.Failure("Failed to delete medical record from database", ServiceErrorType.DatabaseError);
    
        return Result<bool>.Success();
    }

}
