using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IDigitalPrescriptionService
{
    Task<Result<PaginatedResult<DigitalPrescriptionDto>>> GetAll(string? doctorId = null, string? patientId = null, int pageNumber = 1, int pageSize = 10);
    Task<Result<DigitalPrescriptionDto>> GetById(Guid id, string? doctorId = null, string? patientId = null);
    Task<Result<IEnumerable<DigitalPrescriptionDto>>> GetByMedicalRecordId(Guid medicalRecordId, string? patientId = null);
    Task<Result<IEnumerable<DigitalPrescriptionDto>>> GetByPatientId(string patientId, string? doctorId = null);
    Task<Result<DigitalPrescriptionDto>> Create(CreateDigitalPrescriptionDto dto, string? doctorIdForWrite = null);
    Task<Result<DigitalPrescriptionDto>> Update(Guid id, CreateDigitalPrescriptionDto dto, string? doctorIdForWrite = null);
    Task<Result<bool>> Delete(Guid id, string? doctorIdForWrite = null);
}
