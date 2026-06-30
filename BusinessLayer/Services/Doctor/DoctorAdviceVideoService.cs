using AutoMapper;
using DataAccessLayer.Persistence;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusinessLayer.Services;

public class DoctorAdviceVideoService : IDoctorAdviceVideoService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DoctorAdviceVideoService(ApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsSuperAdmin()
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(DomainLayer.Constants.Roles.SuperAdmin) ?? false;
    }

    public async Task<Result<PaginatedResult<DoctorAdviceVideoDto>>> GetAllPublishedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.DoctorAdviceVideos
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .AsNoTracking()
            .Where(v => v.IsPublished)
            .OrderByDescending(v => v.CreatedAt);

        var totalCount = await query.CountAsync();

        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<IEnumerable<DoctorAdviceVideoDto>>(list);
        var paged = PaginatedResult<DoctorAdviceVideoDto>.Create(dtos, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<DoctorAdviceVideoDto>>.Success(paged);
    }

    public async Task<Result<IEnumerable<DoctorAdviceVideoDto>>> GetByDoctorIdAsync(string doctorId)
    {
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<IEnumerable<DoctorAdviceVideoDto>>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);
        var list = await _context.DoctorAdviceVideos
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .AsNoTracking()
            .Where(v => v.DoctorId == doctorId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
        var dtos = _mapper.Map<IEnumerable<DoctorAdviceVideoDto>>(list);
        return Result<IEnumerable<DoctorAdviceVideoDto>>.Success(dtos);
    }

    public async Task<Result<DoctorAdviceVideoDto>> GetByIdAsync(Guid id, string? doctorId = null)
    {
        var video = await _context.DoctorAdviceVideos
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
        if (video == null)
            return Result<DoctorAdviceVideoDto>.Failure("Video not found", ServiceErrorType.NotFound);
        if (!string.IsNullOrEmpty(doctorId) && video.DoctorId != doctorId && !video.IsPublished && !IsSuperAdmin())
            return Result<DoctorAdviceVideoDto>.Failure("Video not found", ServiceErrorType.NotFound);
        if (string.IsNullOrEmpty(doctorId) && !video.IsPublished && !IsSuperAdmin())
            return Result<DoctorAdviceVideoDto>.Failure("Video not found", ServiceErrorType.NotFound);
        var dto = _mapper.Map<DoctorAdviceVideoDto>(video);
        return Result<DoctorAdviceVideoDto>.Success(dto);
    }

    public async Task<Result<DoctorAdviceVideoDto>> CreateAsync(CreateDoctorAdviceVideoDto dto, string doctorId)
    {
        if (dto == null)
            return Result<DoctorAdviceVideoDto>.Failure("Request is required", ServiceErrorType.ValidationError);
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<DoctorAdviceVideoDto>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);
        if (string.IsNullOrWhiteSpace(dto.VideoUrl))
            return Result<DoctorAdviceVideoDto>.Failure("Video file is required", ServiceErrorType.ValidationError);

        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<DoctorAdviceVideoDto>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var entity = new DoctorAdviceVideo
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            Title = dto.Title,
            Description = dto.Description,
            VideoUrl = dto.VideoUrl,
            CreatedAt = DateTime.UtcNow,
            IsPublished = dto.IsPublished
        };
        _context.DoctorAdviceVideos.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.DoctorAdviceVideos
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .AsNoTracking()
            .FirstAsync(v => v.Id == entity.Id);
        var resultDto = _mapper.Map<DoctorAdviceVideoDto>(created);
        return Result<DoctorAdviceVideoDto>.Success(resultDto);
    }

    public async Task<Result<DoctorAdviceVideoDto>> UpdateAsync(Guid id, UpdateDoctorAdviceVideoDto dto, string doctorId)
    {
        if (dto == null)
            return Result<DoctorAdviceVideoDto>.Failure("Request is required", ServiceErrorType.ValidationError);
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<DoctorAdviceVideoDto>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);

        var existing = await _context.DoctorAdviceVideos
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (existing == null)
            return Result<DoctorAdviceVideoDto>.Failure("Video not found", ServiceErrorType.NotFound);
        if (existing.DoctorId != doctorId && !IsSuperAdmin())
            return Result<DoctorAdviceVideoDto>.Failure("You can only update your own advice videos", ServiceErrorType.ValidationError);

        if (dto.Title != null)
            existing.Title = dto.Title;
        if (dto.Description != null)
            existing.Description = dto.Description;
        if (dto.IsPublished.HasValue)
            existing.IsPublished = dto.IsPublished.Value;
        if (!string.IsNullOrWhiteSpace(dto.VideoUrl))
            existing.VideoUrl = dto.VideoUrl;

        await _context.SaveChangesAsync();

        var updated = await _context.DoctorAdviceVideos
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .AsNoTracking()
            .FirstAsync(v => v.Id == id);
        var resultDto = _mapper.Map<DoctorAdviceVideoDto>(updated);
        return Result<DoctorAdviceVideoDto>.Success(resultDto);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, string doctorId)
    {
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<bool>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);

        var existing = await _context.DoctorAdviceVideos.FirstOrDefaultAsync(v => v.Id == id);
        if (existing == null)
            return Result<bool>.Failure("Video not found", ServiceErrorType.NotFound);
        if (existing.DoctorId != doctorId && !IsSuperAdmin())
            return Result<bool>.Failure("You can only delete your own advice videos", ServiceErrorType.ValidationError);

        _context.DoctorAdviceVideos.Remove(existing);
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
