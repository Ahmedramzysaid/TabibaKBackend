using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IDoctorService
{
    Task<Result<PaginatedResult<DoctorDto>>> GetAll(int pageNumber = 1, int pageSize = 10);
    Task<Result<DoctorDto>> GetById(string id);
    Task<Result<DoctorDto>> Add(DoctorDto doctorDto, string password);
    Task<Result<DoctorDto>> Update(DoctorDto patient);
    Task<Result<DoctorDto>> Delete(string id);
    Task<Result<IEnumerable<DoctorDto>>> GetBySpecialization(string specialization);
    
    Task<Result<IEnumerable<string>>> GetAllSpecializations();
    Task<Result<IEnumerable<DoctorSearchDto>>> SearchDoctors(string? specialization = null, string? name = null);
    Task<Result<DashboardStatsDto>> GetDashboardStats(string? doctorId = null);
    Task<Result<IEnumerable<NearbyDoctorDto>>> GetNearbyBySpecialization(
        double latitude, double longitude, string specialization, double maxDistanceKm = 50, string sortBy = "score");
}
