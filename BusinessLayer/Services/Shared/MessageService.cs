using DataAccessLayer.Persistence;
using DomainLayer.DTOs.Chat;
using DomainLayer.Enums;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public MessageService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private async Task<bool> IsParticipantAsync(Guid conversationId, string userId, CancellationToken cancellationToken)
    {
        var conv = await _unitOfWork.Conversations.Find(c => c.Id == conversationId);
        return conv != null && (conv.PatientId == userId || conv.DoctorId == userId);
    }

    public async Task<ChatMessageDto?> SendMessageAsync(Guid conversationId, string senderId, string content, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        if (!isSuperAdmin && !await IsParticipantAsync(conversationId, senderId, cancellationToken))
            return null;
        var conv = await _unitOfWork.Conversations.GetById(conversationId);
        if (conv == null) return null;
        var trimmed = content?.Trim() ?? "";
        if (trimmed.Length == 0) return null;

        var msg = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = trimmed,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _unitOfWork.Messages.Add(msg);
        await _unitOfWork.SaveChanges();

        var recipientId = conv.PatientId == senderId ? conv.DoctorId : conv.PatientId;
        var status = new MessageStatus
        {
            MessageId = msg.Id,
            RecipientId = recipientId,
            Status = MessageDeliveryStatus.Sent,
            UpdatedAtUtc = msg.CreatedAtUtc
        };
        await _unitOfWork.MessageStatuses.Add(status);
        conv.UpdatedAtUtc = msg.CreatedAtUtc;
        _unitOfWork.Conversations.Update(conv);
        await _unitOfWork.SaveChanges();

        var senderName = await _context.Users
            .Where(u => u.Id == senderId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);
        return new ChatMessageDto
        {
            Id = msg.Id,
            ConversationId = conversationId,
            SenderId = senderId,
            SenderName = senderName,
            Content = msg.Content,
            CreatedAtUtc = msg.CreatedAtUtc,
            EditedAtUtc = msg.EditedAtUtc,
            IsDeleted = msg.IsDeleted
        };
    }

    public async Task<MessageHistoryResponseDto> GetHistoryAsync(Guid conversationId, string userId, long? before, int limit, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        if (!isSuperAdmin && !await IsParticipantAsync(conversationId, userId, cancellationToken))
            return new MessageHistoryResponseDto { Messages = new List<ChatMessageDto>() };

        if (limit <= 0) limit = 50;
        if (limit > 100) limit = 100;

        var query = _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted);
        if (before.HasValue)
            query = query.Where(m => m.Id < before.Value);

        var messages = await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(limit + 1)
            .Include(m => m.Sender)
            .ToListAsync(cancellationToken);

        var hasMore = messages.Count > limit;
        if (hasMore) messages = messages.Take(limit).ToList();

        var statuses = await _context.MessageStatuses
            .AsNoTracking()
            .Where(ms => ms.RecipientId == userId && messages.Select(x => x.Id).Contains(ms.MessageId))
            .ToDictionaryAsync(ms => ms.MessageId, ms => ms.Status, cancellationToken);

        var dtos = messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ConversationId = conversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.FullName,
            Content = m.Content,
            CreatedAtUtc = m.CreatedAtUtc,
            EditedAtUtc = m.EditedAtUtc,
            IsDeleted = m.IsDeleted,
            YourStatus = statuses.TryGetValue(m.Id, out var s) ? s : (MessageDeliveryStatus?)null
        }).ToList();

        dtos.Reverse();
        var nextCursor = dtos.Count > 0 ? dtos[0].Id : (long?)null;

        var receivedMessageIds = messages.Where(m => m.SenderId != userId).Select(m => m.Id).ToList();
        if (receivedMessageIds.Count > 0)
            await MarkMessagesAsDeliveredAndReadAsync(conversationId, receivedMessageIds, userId, cancellationToken);

        return new MessageHistoryResponseDto
        {
            Messages = dtos,
            NextCursor = hasMore ? nextCursor : null,
            HasMore = hasMore
        };
    }

    private async Task MarkMessagesAsDeliveredAndReadAsync(Guid conversationId, List<long> messageIds, string userId, CancellationToken cancellationToken)
    {
        if (messageIds.Count == 0) return;
        if (!await IsParticipantAsync(conversationId, userId, cancellationToken)) return;

        var statuses = await _context.MessageStatuses
            .Where(ms => ms.RecipientId == userId && messageIds.Contains(ms.MessageId))
            .ToListAsync(cancellationToken);
        var byMessage = statuses.ToDictionary(ms => ms.MessageId);

        foreach (var messageId in messageIds)
        {
            if (!byMessage.TryGetValue(messageId, out var status))
            {
                status = new MessageStatus
                {
                    MessageId = messageId,
                    RecipientId = userId,
                    Status = MessageDeliveryStatus.Read,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                await _unitOfWork.MessageStatuses.Add(status);
            }
            else
            {
                if (status.Status != MessageDeliveryStatus.Read)
                {
                    status.Status = MessageDeliveryStatus.Read;
                    status.UpdatedAtUtc = DateTime.UtcNow;
                    _unitOfWork.MessageStatuses.Update(status);
                }
            }
        }
        await _unitOfWork.SaveChanges();
    }

    public async Task<ChatMessageDto?> EditMessageAsync(Guid conversationId, long messageId, string userId, string newContent, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newContent)) return null;
        var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId, cancellationToken);
        if (msg == null) return null;
        if (msg.IsDeleted) return null;
        if (!isSuperAdmin && msg.SenderId != userId) return null;
        if (!isSuperAdmin && !await IsParticipantAsync(conversationId, userId, cancellationToken)) return null;

        msg.Content = newContent.Trim();
        msg.EditedAtUtc = DateTime.UtcNow;
        _unitOfWork.Messages.Update(msg);
        await _unitOfWork.SaveChanges();

        var senderName = await _context.Users.Where(u => u.Id == msg.SenderId).Select(u => u.FullName).FirstOrDefaultAsync(cancellationToken);
        return new ChatMessageDto
        {
            Id = msg.Id,
            ConversationId = conversationId,
            SenderId = msg.SenderId,
            SenderName = senderName,
            Content = msg.Content,
            CreatedAtUtc = msg.CreatedAtUtc,
            EditedAtUtc = msg.EditedAtUtc,
            IsDeleted = msg.IsDeleted
        };
    }

    public async Task<bool> DeleteMessageAsync(Guid conversationId, long messageId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        if (!isSuperAdmin && !await IsParticipantAsync(conversationId, userId, cancellationToken))
            return false;

        var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId, cancellationToken);
        if (msg == null) return false;
        if (msg.IsDeleted) return false;
        if (!isSuperAdmin && msg.SenderId != userId) return false;

        msg.IsDeleted = true;
        msg.Content = string.Empty;
        _unitOfWork.Messages.Update(msg);
        await _unitOfWork.SaveChanges();
        return true;
    }

    public async Task<bool> MarkAsDeliveredAsync(Guid conversationId, long messageId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        if (!isSuperAdmin && !await IsParticipantAsync(conversationId, userId, cancellationToken))
            return false;

        var msg = await _unitOfWork.Messages.Find(m => m.Id == messageId && m.ConversationId == conversationId);
        if (msg == null || msg.SenderId == userId) return false;

        var status = await _context.MessageStatuses
            .FirstOrDefaultAsync(ms => ms.MessageId == messageId && ms.RecipientId == userId, cancellationToken);
        if (status == null)
        {
            status = new MessageStatus
            {
                MessageId = messageId,
                RecipientId = userId,
                Status = MessageDeliveryStatus.Delivered,
                UpdatedAtUtc = DateTime.UtcNow
            };
            await _unitOfWork.MessageStatuses.Add(status);
        }
        else if (status.Status == MessageDeliveryStatus.Sent)
        {
            status.Status = MessageDeliveryStatus.Delivered;
            status.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.MessageStatuses.Update(status);
        }
        else
        {
            return false;
        }
        await _unitOfWork.SaveChanges();
        return true;
    }

    public async Task<bool> MarkAsReadAsync(Guid conversationId, long messageId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default)
    {
        if (!isSuperAdmin && !await IsParticipantAsync(conversationId, userId, cancellationToken))
            return false;

        var msg = await _unitOfWork.Messages.Find(m => m.Id == messageId && m.ConversationId == conversationId);
        if (msg == null || msg.SenderId == userId) return false;

        var status = await _context.MessageStatuses
            .FirstOrDefaultAsync(ms => ms.MessageId == messageId && ms.RecipientId == userId, cancellationToken);
        if (status == null)
        {
            status = new MessageStatus
            {
                MessageId = messageId,
                RecipientId = userId,
                Status = MessageDeliveryStatus.Read,
                UpdatedAtUtc = DateTime.UtcNow
            };
            await _unitOfWork.MessageStatuses.Add(status);
        }
        else
        {
            status.Status = MessageDeliveryStatus.Read;
            status.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.MessageStatuses.Update(status);
        }
        await _unitOfWork.SaveChanges();
        return true;
    }
}
