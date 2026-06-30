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

public class DoctorAdvicePostService : IDoctorAdvicePostService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DoctorAdvicePostService(ApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    private bool IsSuperAdmin()
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(DomainLayer.Constants.Roles.SuperAdmin) ?? false;
    }


    public async Task<Result<PaginatedResult<DoctorAdvicePostDto>>> GetAllPublishedAsync(string? currentUserId, int pageNumber = 1, int pageSize = 10)
    {
        var userId = currentUserId ?? GetCurrentUserId();

        var query = _context.DoctorAdvicePosts
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = posts.Select(p => MapToDto(p, userId)).ToList();
        var paged = PaginatedResult<DoctorAdvicePostDto>.Create(dtos, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<DoctorAdvicePostDto>>.Success(paged);
    }

    public async Task<Result<IEnumerable<DoctorAdvicePostDto>>> GetMyPostsAsync(string doctorId)
    {
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<IEnumerable<DoctorAdvicePostDto>>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);

        var posts = await _context.DoctorAdvicePosts
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var dtos = posts.Select(p => MapToDto(p, doctorId)).ToList();
        return Result<IEnumerable<DoctorAdvicePostDto>>.Success(dtos);
    }

    public async Task<Result<DoctorAdvicePostDto>> GetByIdAsync(Guid id, string? currentUserId)
    {
        var userId = currentUserId ?? GetCurrentUserId();

        var post = await _context.DoctorAdvicePosts
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return Result<DoctorAdvicePostDto>.Failure("Post not found", ServiceErrorType.NotFound);
        if (!post.IsPublished && post.DoctorId != userId && !IsSuperAdmin())
            return Result<DoctorAdvicePostDto>.Failure("Post not found", ServiceErrorType.NotFound);

        return Result<DoctorAdvicePostDto>.Success(MapToDto(post, userId));
    }

    public async Task<Result<DoctorAdvicePostDto>> CreateAsync(CreateDoctorAdvicePostDto dto, string doctorId, string? imageUrl)
    {
        if (dto == null)
            return Result<DoctorAdvicePostDto>.Failure("Request is required", ServiceErrorType.ValidationError);
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<DoctorAdvicePostDto>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);
        if (string.IsNullOrWhiteSpace(dto.Content))
            return Result<DoctorAdvicePostDto>.Failure("Content is required", ServiceErrorType.ValidationError);

        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
        if (!doctorExists)
            return Result<DoctorAdvicePostDto>.Failure("Doctor not found", ServiceErrorType.NotFound);

        var entity = new DoctorAdvicePost
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            Content = dto.Content,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            IsPublished = dto.IsPublished
        };

        _context.DoctorAdvicePosts.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.DoctorAdvicePosts
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .FirstAsync(p => p.Id == entity.Id);

        return Result<DoctorAdvicePostDto>.Success(MapToDto(created, doctorId));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, string doctorId)
    {
        if (string.IsNullOrWhiteSpace(doctorId))
            return Result<bool>.Failure("Doctor ID is required", ServiceErrorType.ValidationError);

        var existing = await _context.DoctorAdvicePosts.FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null)
            return Result<bool>.Failure("Post not found", ServiceErrorType.NotFound);
        if (existing.DoctorId != doctorId && !IsSuperAdmin())
            return Result<bool>.Failure("You can only delete your own posts", ServiceErrorType.ValidationError);

        _context.DoctorAdvicePosts.Remove(existing);
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }


    public async Task<Result<bool>> ToggleLikeAsync(Guid postId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Failure("User ID is required", ServiceErrorType.ValidationError);

        var postExists = await _context.DoctorAdvicePosts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            return Result<bool>.Failure("Post not found", ServiceErrorType.NotFound);

        var existingLike = await _context.AdviceLikes
            .FirstOrDefaultAsync(l => l.AdvicePostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            _context.AdviceLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(false); // false = unliked
        }
        else
        {
            var like = new AdviceLike
            {
                AdvicePostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.AdviceLikes.Add(like);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true); // true = liked
        }
    }

    public async Task<Result<IEnumerable<AdviceLikeDto>>> GetLikesAsync(Guid postId)
    {
        var postExists = await _context.DoctorAdvicePosts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            return Result<IEnumerable<AdviceLikeDto>>.Failure("Post not found", ServiceErrorType.NotFound);

        var likes = await _context.AdviceLikes
            .Include(l => l.User)
            .AsNoTracking()
            .Where(l => l.AdvicePostId == postId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new AdviceLikeDto
            {
                UserId = l.UserId,
                UserName = l.User.FullName,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Result<IEnumerable<AdviceLikeDto>>.Success(likes);
    }


    public async Task<Result<AdviceCommentDto>> AddCommentAsync(Guid postId, CreateAdviceCommentDto dto, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<AdviceCommentDto>.Failure("User ID is required", ServiceErrorType.ValidationError);
        if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            return Result<AdviceCommentDto>.Failure("Comment content is required", ServiceErrorType.ValidationError);

        var postExists = await _context.DoctorAdvicePosts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            return Result<AdviceCommentDto>.Failure("Post not found", ServiceErrorType.NotFound);

        var comment = new AdviceComment
        {
            AdvicePostId = postId,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdviceComments.Add(comment);
        await _context.SaveChangesAsync();

        var created = await _context.AdviceComments
            .Include(c => c.User)
            .AsNoTracking()
            .FirstAsync(c => c.Id == comment.Id);

        return Result<AdviceCommentDto>.Success(MapCommentToDto(created));
    }

    public async Task<Result<AdviceCommentDto>> UpdateCommentAsync(long commentId, UpdateAdviceCommentDto dto, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<AdviceCommentDto>.Failure("User ID is required", ServiceErrorType.ValidationError);
        if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            return Result<AdviceCommentDto>.Failure("Comment content is required", ServiceErrorType.ValidationError);

        var comment = await _context.AdviceComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
            return Result<AdviceCommentDto>.Failure("Comment not found", ServiceErrorType.NotFound);
        if (comment.UserId != userId && !IsSuperAdmin())
            return Result<AdviceCommentDto>.Failure("You can only edit your own comments", ServiceErrorType.ValidationError);

        comment.Content = dto.Content;
        comment.EditedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<AdviceCommentDto>.Success(MapCommentToDto(comment));
    }

    public async Task<Result<bool>> DeleteCommentAsync(long commentId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Failure("User ID is required", ServiceErrorType.ValidationError);

        var comment = await _context.AdviceComments
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
            return Result<bool>.Failure("Comment not found", ServiceErrorType.NotFound);
        if (comment.UserId != userId && !IsSuperAdmin())
            return Result<bool>.Failure("You can only delete your own comments", ServiceErrorType.ValidationError);

        comment.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<AdviceCommentDto>>> GetCommentsAsync(Guid postId)
    {
        var postExists = await _context.DoctorAdvicePosts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            return Result<IEnumerable<AdviceCommentDto>>.Failure("Post not found", ServiceErrorType.NotFound);

        var comments = await _context.AdviceComments
            .Include(c => c.User)
            .AsNoTracking()
            .Where(c => c.AdvicePostId == postId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var dtos = comments.Select(MapCommentToDto).ToList();
        return Result<IEnumerable<AdviceCommentDto>>.Success(dtos);
    }


    public async Task<Result<bool>> ToggleSavePostAsync(Guid postId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Failure("User ID is required", ServiceErrorType.ValidationError);

        var postExists = await _context.DoctorAdvicePosts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            return Result<bool>.Failure("Post not found", ServiceErrorType.NotFound);

        var existingSave = await _context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.DoctorAdvicePostId == postId && sp.UserId == userId);

        if (existingSave != null)
        {
            _context.SavedPosts.Remove(existingSave);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(false); // false = unsaved
        }
        else
        {
            var savedPost = new SavedPost
            {
                DoctorAdvicePostId = postId,
                UserId = userId,
                SavedAt = DateTime.UtcNow
            };
            _context.SavedPosts.Add(savedPost);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true); // true = saved
        }
    }

    public async Task<Result<IEnumerable<SavedPostDto>>> GetSavedPostsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<IEnumerable<SavedPostDto>>.Failure("User ID is required", ServiceErrorType.ValidationError);

        var savedPosts = await _context.SavedPosts
            .Include(sp => sp.DoctorAdvicePost).ThenInclude(p => p.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .Select(sp => new SavedPostDto
            {
                PostId = sp.DoctorAdvicePostId,
                Content = sp.DoctorAdvicePost.Content,
                ImageUrl = sp.DoctorAdvicePost.ImageUrl,
                DoctorName = sp.DoctorAdvicePost.Doctor.User.FullName,
                SavedByUserId = sp.UserId,
                SavedAt = sp.SavedAt
            })
            .ToListAsync();

        return Result<IEnumerable<SavedPostDto>>.Success(savedPosts);
    }


    private DoctorAdvicePostDto MapToDto(DoctorAdvicePost post, string? currentUserId)
    {
        return new DoctorAdvicePostDto
        {
            Id = post.Id,
            DoctorId = post.DoctorId,
            DoctorName = post.Doctor?.User?.FullName,
            DoctorSpecialization = post.Doctor?.Specialization,
            DoctorProfileImageUrl = post.Doctor?.User?.ProfileImageUrl,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            IsPublished = post.IsPublished,
            LikesCount = post.Likes?.Count ?? 0,
            CommentsCount = post.Comments?.Count(c => !c.IsDeleted) ?? 0,
            IsLikedByCurrentUser = !string.IsNullOrEmpty(currentUserId) && (post.Likes?.Any(l => l.UserId == currentUserId) ?? false)
        };
    }

    private static AdviceCommentDto MapCommentToDto(AdviceComment comment)
    {
        return new AdviceCommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = comment.User?.FullName,
            UserProfileImageUrl = comment.User?.ProfileImageUrl,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            EditedAt = comment.EditedAt
        };
    }
}
