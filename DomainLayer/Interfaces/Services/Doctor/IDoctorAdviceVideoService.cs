using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IDoctorAdviceVideoService
{
    Task<Result<PaginatedResult<DoctorAdviceVideoDto>>> GetAllPublishedAsync(int pageNumber = 1, int pageSize = 10);

    Task<Result<IEnumerable<DoctorAdviceVideoDto>>> GetByDoctorIdAsync(string doctorId);

    Task<Result<DoctorAdviceVideoDto>> GetByIdAsync(Guid id, string? doctorId = null);

    Task<Result<DoctorAdviceVideoDto>> CreateAsync(CreateDoctorAdviceVideoDto dto, string doctorId);

    Task<Result<DoctorAdviceVideoDto>> UpdateAsync(Guid id, UpdateDoctorAdviceVideoDto dto, string doctorId);

    Task<Result<bool>> DeleteAsync(Guid id, string doctorId);
}
