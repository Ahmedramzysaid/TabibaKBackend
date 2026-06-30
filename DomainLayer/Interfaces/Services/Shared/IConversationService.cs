using DomainLayer.DTOs.Chat;

namespace DomainLayer.Interfaces.Services;

public interface IConversationService
{
    Task<ConversationDto?> CreateOrGetAsync(string userId, CreateConversationRequestDto request, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationDto>> GetMyConversationsAsync(string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationDto>> GetConversationsByParticipantIdsAsync(string? patientId, string? doctorId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<ConversationDto?> GetByIdAsync(Guid conversationId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAllowedConversationIdsForUserAsync(string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);
}
