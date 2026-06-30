using ClinicAPI.DTOs;
using ClinicAPI.Repositories;

namespace ClinicAPI.Services;

public class MedicineService : IMedicineService
{
    private readonly IMedicineRepository _repository;

    public MedicineService(IMedicineRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedMedicinesResponseDto> SearchAsync(string? name, string? arabicName, string? activeIngredient, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _repository.SearchAsync(name, arabicName, activeIngredient, pageNumber, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedMedicinesResponseDto
        {
            Items = items.Select(m => new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                ArabicName = m.ArabicName,
                Price = m.Price,
                Company = m.Company,
                ActiveIngredient = m.ActiveIngredient,
                Description = m.Description,
                ProductUrl = m.ProductUrl
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }


}
