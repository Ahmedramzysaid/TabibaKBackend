using AutoMapper;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly DataAccessLayer.Persistence.ApplicationDbContext _context;

    public PrescriptionService(IUnitOfWork unitOfWork, IMapper mapper, DataAccessLayer.Persistence.ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PaginatedResult<PrescriptionDto>>> GetAll(string? doctorId = null, string? patientId = null, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Prescriptions.Include(p => p.MedicalRecord).AsNoTracking();

        if (!string.IsNullOrEmpty(doctorId))
        {
            var patientIdsWithAppointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Select(a => a.PatientID)
                .Distinct()
                .ToListAsync();
            query = query.Where(p => p.MedicalRecord != null && patientIdsWithAppointments.Contains(p.MedicalRecord.PatientId));
        }
        if (!string.IsNullOrEmpty(patientId))
            query = query.Where(p => p.MedicalRecord != null && p.MedicalRecord.PatientId == patientId);

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
            return Result<PaginatedResult<PrescriptionDto>>.Failure("No prescriptions found", ServiceErrorType.NotFound);

        var prescriptions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var prescriptionDtos = _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
        var paged = PaginatedResult<PrescriptionDto>.Create(prescriptionDtos, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<PrescriptionDto>>.Success(paged);
    }

    public async Task<Result<PrescriptionDto>> GetById(Guid id, string? doctorId = null, string? patientId = null)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null)
            return Result<PrescriptionDto>.Failure("Prescription not found", ServiceErrorType.NotFound);

        if (!string.IsNullOrEmpty(doctorId) && prescription.MedicalRecord != null)
        {
            var hasAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorID == doctorId && a.PatientID == prescription.MedicalRecord.PatientId);
            if (!hasAppointment)
                return Result<PrescriptionDto>.Failure("Prescription not found", ServiceErrorType.NotFound);
        }
        if (!string.IsNullOrEmpty(patientId) && prescription.MedicalRecord != null && prescription.MedicalRecord.PatientId != patientId)
            return Result<PrescriptionDto>.Failure("Prescription not found", ServiceErrorType.NotFound);

        var prescriptionDto = _mapper.Map<PrescriptionDto>(prescription);
        return Result<PrescriptionDto>.Success(prescriptionDto);
    }

    public async Task<Result<IEnumerable<PrescriptionDto>>> GetByMedicalRecordId(Guid medicalRecordId, string? patientId = null)
    {
        var query = _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .Where(p => p.MedicalRecordId == medicalRecordId);
        if (!string.IsNullOrEmpty(patientId))
        {
            var mr = await _context.MedicalRecords.AsNoTracking().FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);
            if (mr?.PatientId != patientId)
                return Result<IEnumerable<PrescriptionDto>>.Failure("Medical record not found", ServiceErrorType.NotFound);
        }
        var prescriptions = await query.ToListAsync();

        if (prescriptions == null || !prescriptions.Any())
            return Result<IEnumerable<PrescriptionDto>>.Failure("No prescriptions found for this medical record", ServiceErrorType.NotFound);

        var prescriptionDtos = _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
        return Result<IEnumerable<PrescriptionDto>>.Success(prescriptionDtos);
    }

    public async Task<Result<IEnumerable<PrescriptionDto>>> GetByPatientId(string patientId, string? doctorId = null)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return Result<IEnumerable<PrescriptionDto>>.Failure("Patient ID is required", ServiceErrorType.ValidationError);

        var query = _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .Where(p => p.MedicalRecord != null && p.MedicalRecord.PatientId == patientId);

        if (!string.IsNullOrEmpty(doctorId))
        {
            var patientIdsWithAppointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Select(a => a.PatientID)
                .Distinct()
                .ToListAsync();
            query = query.Where(p => p.MedicalRecord != null && patientIdsWithAppointments.Contains(p.MedicalRecord.PatientId));
        }

        var prescriptions = await query.OrderByDescending(p => p.MedicalRecordId).ToListAsync();
        var prescriptionDtos = _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
        return Result<IEnumerable<PrescriptionDto>>.Success(prescriptionDtos);
    }

    public async Task<Result<PrescriptionDto>> Add(CreatePrescriptionDto createPrescriptionDto)
    {
        return await Add(createPrescriptionDto, null, null);
    }

    public async Task<Result<PrescriptionDto>> Add(CreatePrescriptionDto createPrescriptionDto, string? filePath, string? fileType)
    {
        var medicalRecordExists = await _context.MedicalRecords
            .AnyAsync(m => m.MedicalRecordId == createPrescriptionDto.MedicalRecordId);
        if (!medicalRecordExists)
            return Result<PrescriptionDto>.Failure("Medical record not found", ServiceErrorType.NotFound);

        var prescription = _mapper.Map<Prescription>(createPrescriptionDto);
        prescription.PrescriptionId = Guid.NewGuid();
        if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(fileType))
        {
            prescription.FilePath = filePath;
            prescription.FileType = fileType;
        }

        await _unitOfWork.Prescriptions.Add(prescription);
        bool saveResult = await _unitOfWork.SaveChanges();
        if (!saveResult)
            return Result<PrescriptionDto>.Failure("Failed to add prescription to database", ServiceErrorType.DatabaseError);

        var createdPrescription = await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescription.PrescriptionId);

        var prescriptionDto = _mapper.Map<PrescriptionDto>(createdPrescription);
        return Result<PrescriptionDto>.Success(prescriptionDto);
    }

    public async Task<Result<PrescriptionDto>> Update(PrescriptionDto prescriptionDto)
    {
        return await Update(prescriptionDto, null, null);
    }

    public async Task<Result<PrescriptionDto>> Update(PrescriptionDto prescriptionDto, string? filePath, string? fileType)
    {
        var existingPrescription = await _unitOfWork.Prescriptions.GetById(prescriptionDto.PrescriptionId);
        if (existingPrescription == null)
            return Result<PrescriptionDto>.Failure("Prescription not found", ServiceErrorType.NotFound);

        existingPrescription.Title = prescriptionDto.Title;
        existingPrescription.Status = prescriptionDto.Status;
        if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(fileType))
        {
            existingPrescription.FilePath = filePath;
            existingPrescription.FileType = fileType;
        }

        _unitOfWork.Prescriptions.Update(existingPrescription);
        bool saveResult = await _unitOfWork.SaveChanges();
        if (!saveResult)
            return Result<PrescriptionDto>.Failure("Failed to update prescription in database", ServiceErrorType.DatabaseError);

        var updatedPrescription = await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionDto.PrescriptionId);

        var updatedDto = _mapper.Map<PrescriptionDto>(updatedPrescription);
        return Result<PrescriptionDto>.Success(updatedDto);
    }

    public async Task<Result<bool>> Delete(Guid id)
    {
        var prescription = await _unitOfWork.Prescriptions.GetById(id);
        if (prescription == null)
            return Result<bool>.Failure("Prescription not found", ServiceErrorType.NotFound);

        _unitOfWork.Prescriptions.Delete(prescription);
        bool saveResult = await _unitOfWork.SaveChanges();
        if (!saveResult)
            return Result<bool>.Failure("Failed to delete prescription from database", ServiceErrorType.DatabaseError);
        return Result<bool>.Success(true);
    }
}
