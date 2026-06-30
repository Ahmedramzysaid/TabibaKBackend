using ClinicAPI.DTOs;

namespace ClinicAPI.Services;

public interface IMedicineService
{
    Task<PagedMedicinesResponseDto> SearchAsync(string? name, string? arabicName, string? activeIngredient, int pageNumber, int pageSize);


}
