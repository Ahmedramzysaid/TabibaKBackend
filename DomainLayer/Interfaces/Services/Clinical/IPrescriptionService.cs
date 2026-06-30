using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IPrescriptionService
{
    Task<Result<PaginatedResult<PrescriptionDto>>> GetAll(string? doctorId = null, string? patientId = null, int pageNumber = 1, int pageSize = 10);
    
    Task<Result<PrescriptionDto>> GetById(Guid id, string? doctorId = null, string? patientId = null);
    
    Task<Result<IEnumerable<PrescriptionDto>>> GetByMedicalRecordId(Guid medicalRecordId, string? patientId = null);

    Task<Result<IEnumerable<PrescriptionDto>>> GetByPatientId(string patientId, string? doctorId = null);
    
    Task<Result<PrescriptionDto>> Add(CreatePrescriptionDto createPrescriptionDto);
    
    Task<Result<PrescriptionDto>> Update(PrescriptionDto prescriptionDto);
    
    Task<Result<bool>> Delete(Guid id);
}
