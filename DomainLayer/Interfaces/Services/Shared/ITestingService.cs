using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface ITestingService
{
    Task<Result<PaginatedResult<TestingDto>>> GetAll(string? doctorId = null, int pageNumber = 1, int pageSize = 10);
    
    Task<Result<TestingDto>> GetById(Guid id, string? doctorId = null);
    
    Task<Result<IEnumerable<TestingDto>>> GetByMedicalRecordId(Guid medicalRecordId);

    Task<Result<IEnumerable<TestingDto>>> GetByPatientId(string patientId, string? doctorId = null);
    
    Task<Result<TestingDto>> Add(CreateTestingDto createTestingDto);
    
    Task<Result<TestingDto>> Update(TestingDto testingDto);
    
    Task<Result<bool>> Delete(Guid id);
}
