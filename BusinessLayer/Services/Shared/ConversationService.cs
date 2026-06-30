using DataAccessLayer.Persistence;
using DomainLayer.DTOs.Chat;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public ConversationService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private async Task<bool> HasAssignmentAsync(string patientId, string doctorId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Appointments.ExistsAsync(a => a.PatientID == patientId && a.DoctorID == doctorId);
    }

    private async Task<bool> IsDoctorAsync(string userId, CancellationToken cancellationToken)
    {
        var hasDoctor = await _unitOfWork.Doctors.ExistsAsync(d => d.Id == userId);
        if (hasDoctor) return true;
        return false;
    }

    public async Task<ConversationDto?> CreateOrGetAsync(string userId, CreateConversationRequestDto request, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        string? patientId;
        string? doctorId;
        if (isSuperAdmin && !string.IsNullOrWhiteSpace(request.PatientId) && !string.IsNullOrWhiteSpace(request.DoctorId))
        {
            patientId = request.PatientId.Trim();
            doctorId = request.DoctorId.Trim();
        }
        else if (await IsDoctorAsync(userId, cancellationToken))
        {
            doctorId = userId;
            patientId = request.PatientId?.Trim();
            if (string.IsNullOrEmpty(patientId))
                return null;
        }
        else
        {
            patientId = userId;
            doctorId = request.DoctorId?.Trim();
            if (string.IsNullOrEmpty(doctorId))
                return null;
        }

        if (!isSuperAdmin && !await HasAssignmentAsync(patientId, doctorId, cancellationToken))
            return null;

        var existing = await _unitOfWork.Conversations.Find(c =>
            c.PatientId == patientId && c.DoctorId == doctorId);
        if (existing != null)
            return await MapToDtoAsync(existing, userId, cancellationToken);

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        await _unitOfWork.Conversations.Add(conversation);
        await _unitOfWork.SaveChanges();

        await WriteAuditAsync(conversation.Id, userId, "ConversationCreated", null, null, cancellationToken);

        return await MapToDtoAsync(conversation, userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationDto>> GetMyConversationsAsync(string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        var isDoctor = await IsDoctorAsync(userId, cancellationToken);
        var query = isSuperAdmin
            ? _context.Conversations.AsQueryable()
            : isDoctor
                ? _context.Conversations.Where(c => c.DoctorId == userId)
                : _context.Conversations.Where(c => c.PatientId == userId);

        var list = await query
            .OrderByDescending(c => c.UpdatedAtUtc)
            .Include(c => c.Patient).ThenInclude(p => p.User)
            .Include(c => c.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<ConversationDto>();
        foreach (var c in list)
        {
            var dto = await MapToDtoAsync(c, userId, cancellationToken);
            if (dto != null)
                result.Add(dto);
        }
        return result;
    }

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsByParticipantIdsAsync(string? patientId, string? doctorId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Conversation> query = _context.Conversations.AsQueryable();

        if (!isSuperAdmin)
        {
            var isDoctor = await IsDoctorAsync(userId, cancellationToken);
            if (isDoctor)
                query = query.Where(c => c.DoctorId == userId);
            else
                query = query.Where(c => c.PatientId == userId);
        }

        if (!string.IsNullOrWhiteSpace(patientId))
            query = query.Where(c => c.PatientId == patientId.Trim());
        if (!string.IsNullOrWhiteSpace(doctorId))
            query = query.Where(c => c.DoctorId == doctorId.Trim());

        var list = await query
            .OrderByDescending(c => c.UpdatedAtUtc)
            .Include(c => c.Patient).ThenInclude(p => p.User)
            .Include(c => c.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<ConversationDto>();
        foreach (var c in list)
        {
            var dto = await MapToDtoAsync(c, userId, cancellationToken);
            if (dto != null)
                result.Add(dto);
        }
        return result;
    }

    public async Task<ConversationDto?> GetByIdAsync(Guid conversationId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        var conv = await _context.Conversations
            .Include(c => c.Patient).ThenInclude(p => p.User)
            .Include(c => c.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        if (conv == null)
            return null;
        if (!isSuperAdmin && conv.PatientId != userId && conv.DoctorId != userId)
            return null;
        return await MapToDtoAsync(conv, userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetAllowedConversationIdsForUserAsync(string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        var query = isSuperAdmin
            ? _context.Conversations.AsNoTracking().Select(c => c.Id)
            : _context.Conversations.AsNoTracking().Where(c => c.PatientId == userId || c.DoctorId == userId).Select(c => c.Id);
        var list = await query.ToListAsync(cancellationToken);
        return list;
    }

    private async Task<ConversationDto?> MapToDtoAsync(Conversation c, string currentUserId, CancellationToken cancellationToken)
    {
        if (c.Patient == null || c.Doctor == null)
        {
            var id = c.Id;
            c = await _context.Conversations
                .Include(x => x.Patient).ThenInclude(p => p.User)
                .Include(x => x.Doctor).ThenInclude(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? c;
        }
        var lastMessage = await _context.Messages
            .Where(m => m.ConversationId == c.Id)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(1)
            .Select(m => new { m.Id, m.SenderId, m.Content, m.CreatedAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        var unreadCount = 0;
        if (lastMessage != null)
        {
            var read = await _context.MessageStatuses
                .AnyAsync(ms => ms.MessageId == lastMessage.Id && ms.RecipientId == currentUserId && ms.Status == DomainLayer.Enums.MessageDeliveryStatus.Read, cancellationToken);
            if (!read)
            {
                unreadCount = await _context.Messages
                    .CountAsync(m => m.ConversationId == c.Id && m.SenderId != currentUserId &&
                        !_context.MessageStatuses.Any(ms => ms.MessageId == m.Id && ms.RecipientId == currentUserId && ms.Status == DomainLayer.Enums.MessageDeliveryStatus.Read), cancellationToken);
            }
        }

        var patientName = c.Patient?.User?.FullName ?? "";
        var doctorName = c.Doctor?.User?.FullName ?? "";

        ChatMessageDto? lastMsgDto = null;
        if (lastMessage != null)
        {
            lastMsgDto = new ChatMessageDto
            {
                Id = lastMessage.Id,
                ConversationId = c.Id,
                SenderId = lastMessage.SenderId,
                Content = lastMessage.Content,
                CreatedAtUtc = lastMessage.CreatedAtUtc
            };
        }

        return new ConversationDto
        {
            Id = c.Id,
            PatientId = c.PatientId,
            DoctorId = c.DoctorId,
            PatientName = patientName,
            DoctorName = doctorName,
            CreatedAtUtc = c.CreatedAtUtc,
            UpdatedAtUtc = c.UpdatedAtUtc,
            LastMessage = lastMsgDto,
            UnreadCount = unreadCount
        };
    }

    private async Task WriteAuditAsync(Guid conversationId, string actorId, string action, long? messageId, string? metadata, CancellationToken cancellationToken)
    {
        try
        {
            var log = new ChatAuditLog
            {
                ConversationId = conversationId,
                ActorId = actorId,
                Action = action,
                MessageId = messageId,
                Metadata = metadata,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _unitOfWork.ChatAuditLogs.Add(log);
            await _unitOfWork.SaveChanges();
        }
        catch { /* best effort */ }
    }

}
