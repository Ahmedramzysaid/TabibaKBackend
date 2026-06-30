using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IDoctorAdvicePostService
{
    Task<Result<PaginatedResult<DoctorAdvicePostDto>>> GetAllPublishedAsync(string? currentUserId, int pageNumber = 1, int pageSize = 10);

    Task<Result<IEnumerable<DoctorAdvicePostDto>>> GetMyPostsAsync(string doctorId);

    Task<Result<DoctorAdvicePostDto>> GetByIdAsync(Guid id, string? currentUserId);

    Task<Result<DoctorAdvicePostDto>> CreateAsync(CreateDoctorAdvicePostDto dto, string doctorId, string? imageUrl);

    Task<Result<bool>> DeleteAsync(Guid id, string doctorId);

    Task<Result<bool>> ToggleLikeAsync(Guid postId, string userId);

    Task<Result<IEnumerable<AdviceLikeDto>>> GetLikesAsync(Guid postId);

    Task<Result<AdviceCommentDto>> AddCommentAsync(Guid postId, CreateAdviceCommentDto dto, string userId);

    Task<Result<AdviceCommentDto>> UpdateCommentAsync(long commentId, UpdateAdviceCommentDto dto, string userId);

    Task<Result<bool>> DeleteCommentAsync(long commentId, string userId);

    Task<Result<IEnumerable<AdviceCommentDto>>> GetCommentsAsync(Guid postId);

    Task<Result<bool>> ToggleSavePostAsync(Guid postId, string userId);

    Task<Result<IEnumerable<SavedPostDto>>> GetSavedPostsAsync(string userId);
}
