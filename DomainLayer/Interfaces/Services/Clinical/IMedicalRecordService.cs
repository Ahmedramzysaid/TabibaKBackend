using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IMedicalRecordService
{
    Task<Result<PaginatedResult<MedicalRecordDto>>> GetAll(string? doctorId = null, int pageNumber = 1, int pageSize = 10);
    
    Task<Result<MedicalRecordDto>> GetById(Guid id, string? doctorId = null);
    
    Task<Result<MedicalRecordDto>> GetByPatientId(string patientId);
    
    Task<Result<MedicalRecordDto>> Add(CreateMedicalRecordDto createMedicalRecordDto);
    
    Task<Result<bool>> Update(MedicalRecordDto medicalRecordDto);
    
    Task<Result<bool>> Delete(Guid id);
    
}
