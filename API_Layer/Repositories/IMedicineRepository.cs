using DomainLayer.Models;

namespace ClinicAPI.Repositories;

public interface IMedicineRepository
{
    Task<(IReadOnlyList<Medicine> Items, int TotalCount)> SearchAsync(string? name, string? arabicName, string? activeIngredient, int pageNumber, int pageSize);


}
