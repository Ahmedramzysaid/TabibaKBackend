using ClinicAPI.Helpers;
using DataAccessLayer.Persistence;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicAPI.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public MedicineRepository(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<(IReadOnlyList<Medicine> Items, int TotalCount)> SearchAsync(
        string? name, string? arabicName, string? activeIngredient, int pageNumber, int pageSize)
    {
        var cacheKey = $"med:search:{name?.ToLowerInvariant()}:{arabicName?.ToLowerInvariant()}:{activeIngredient?.ToLowerInvariant()}:{pageNumber}:{pageSize}";

        if (_cache.TryGetValue(cacheKey, out (IReadOnlyList<Medicine> Items, int TotalCount) cached))
        {
            return cached;
        }

        var query = _context.Medicines.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var n = name.Trim();
            query = query.Where(m => m.Name.Contains(n));
        }

        if (!string.IsNullOrWhiteSpace(arabicName))
        {
            var normalizedSearch = ArabicNormalization.Normalize(arabicName);
            var rawSearch = arabicName.Trim();

            if (normalizedSearch.Length > 0)
            {
                query = query.Where(m =>
                    m.ArabicNameNormalized.Contains(normalizedSearch) ||
                    m.ArabicName.Contains(rawSearch));
            }
            else
            {
                query = query.Where(m => m.ArabicName.Contains(rawSearch));
            }
        }

        if (!string.IsNullOrWhiteSpace(activeIngredient))
        {
            var ai = activeIngredient.Trim();
            query = query.Where(m => m.ActiveIngredient != null && m.ActiveIngredient.Contains(ai));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(m => m.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = ((IReadOnlyList<Medicine>)items, totalCount);

        _cache.Set(cacheKey, result, CacheDuration);

        return result;
    }


}
