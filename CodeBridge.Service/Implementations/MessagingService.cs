using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Messaging;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>منطق کسب‌وکار گفت‌وگو: ادمین-کاربر و گروه پروژه.</summary>
public sealed class MessagingService : IMessagingService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationParticipantRepository _participantRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IAuditLogService _auditLogService;

    public MessagingService(
        IConversationRepository conversationRepository,
        IConversationParticipantRepository participantRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IAuditLogService auditLogService)
    {
        _conversationRepository = conversationRepository
            ?? throw new ArgumentNullException(nameof(conversationRepository));
        _participantRepository = participantRepository
            ?? throw new ArgumentNullException(nameof(participantRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _projectMemberRepository = projectMemberRepository
            ?? throw new ArgumentNullException(nameof(projectMemberRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Result<ConversationDto>> CreateDirectAsync(
        int adminId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return Result<ConversationDto>.Failure(adminCheck.Message ?? "ادمین معتبر نیست.");

        if (userId <= 0)
            return Result<ConversationDto>.Failure("شناسه‌ی کاربر معتبر نیست.");

        if (adminId == userId)
            return Result<ConversationDto>.Failure("امکان شروع گفت‌وگو با خودتان نیست.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result<ConversationDto>.Failure("کاربر مورد نظر پیدا نشد.");

        // اگر گفت‌وگوی قبلی بین همین دو نفر وجود داشته باشد همان بازگردانده می‌شود
        var existing = await FindDirectAsync(adminId, userId, cancellationToken);

        if (existing is not null)
            return Result<ConversationDto>.Success(ConversationMapper.ToDto(existing));

        var conversation = Conversation.CreateAdminUser(UserMapper.DisplayName(user));

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        await AddParticipantAsync(conversation.Id, adminId, cancellationToken);
        await AddParticipantAsync(conversation.Id, userId, cancellationToken);
        await _participantRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ConversationCreated",
            nameof(Conversation),
            conversation.Id,
            "AdminUser",
            null,
            cancellationToken);

        var created = await _conversationRepository.GetWithParticipantsAsync(
            conversation.Id,
            cancellationToken);

        return created is null
            ? Result<ConversationDto>.Failure("ساخت مکالمه ناموفق بود.")
            : Result<ConversationDto>.Success(ConversationMapper.ToDto(created));
    }

    public async Task<Result<ConversationDto>> CreateProjectGroupAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return Result<ConversationDto>.Failure(adminCheck.Message ?? "ادمین معتبر نیست.");

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
            return Result<ConversationDto>.Failure("پروژه‌ی مورد نظر پیدا نشد.");

        if (await _conversationRepository.GetProjectGroupAsync(projectId, cancellationToken) is not null)
            return Result<ConversationDto>.Failure("برای این پروژه قبلاً گروه گفت‌وگو ساخته شده است.");

        var conversation = Conversation.CreateProjectGroup(project.Id, project.Title);

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        // اعضای گروه: ادمین سازنده، کارفرما، برنامه‌نویس تخصیص‌یافته و اعضای تیم پروژه
        var participantIds = new HashSet<int> { adminId, project.EmployerId };

        if (project.AssignedDeveloperId is int developerId)
            participantIds.Add(developerId);

        foreach (var member in await _projectMemberRepository.GetByProjectIdAsync(projectId, cancellationToken))
            participantIds.Add(member.UserId);

        foreach (var participantId in participantIds)
            await AddParticipantAsync(conversation.Id, participantId, cancellationToken);

        await _participantRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminId,
            "ConversationCreated",
            nameof(Conversation),
            conversation.Id,
            $"ProjectGroup;ProjectId={projectId}",
            null,
            cancellationToken);

        var created = await _conversationRepository.GetWithParticipantsAsync(
            conversation.Id,
            cancellationToken);

        return created is null
            ? Result<ConversationDto>.Failure("ساخت گروه پروژه ناموفق بود.")
            : Result<ConversationDto>.Success(ConversationMapper.ToDto(created));
    }

    public async Task<Result<ConversationDto>> AddUserToProjectGroupAsync(
        int conversationId,
        int userId,
        int adminId,
        CancellationToken cancellationToken = default)
    {
        var adminCheck = await ValidateAdminAsync(adminId, cancellationToken);

        if (!adminCheck.IsSuccess)
            return Result<ConversationDto>.Failure(adminCheck.Message ?? "ادمین معتبر نیست.");

        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
            return Result<ConversationDto>.Failure("مکالمه مورد نظر پیدا نشد.");

        if (conversation.Type != ConversationType.ProjectGroup)
            return Result<ConversationDto>.Failure("افزودن عضو فقط برای گروه پروژه ممکن است.");

        if (await _userRepository.GetByIdAsync(userId, cancellationToken) is null)
            return Result<ConversationDto>.Failure("کاربر مورد نظر پیدا نشد.");

        var added = await _participantRepository.AddIfMissingAsync(
            conversationId,
            userId,
            cancellationToken);

        if (added)
            await _participantRepository.SaveChangesAsync(cancellationToken);

        var updated = await _conversationRepository.GetWithParticipantsAsync(
            conversationId,
            cancellationToken);

        return updated is null
            ? Result<ConversationDto>.Failure("مکالمه مورد نظر پیدا نشد.")
            : Result<ConversationDto>.Success(ConversationMapper.ToDto(updated));
    }

    public async Task<Result<IReadOnlyList<ConversationDto>>> GetMyConversationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetForUserAsync(userId, cancellationToken);

        var dtos = new List<ConversationDto>(conversations.Count);

        foreach (var conversation in conversations)
        {
            var dto = ConversationMapper.ToDto(conversation);
            dto.UnreadCount = await GetUnreadCountAsync(conversation, userId, cancellationToken);
            dtos.Add(dto);
        }

        return Result<IReadOnlyList<ConversationDto>>.Success(dtos);
    }

    public async Task<Result<ConversationDto>> GetByIdAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetWithParticipantsAsync(
            conversationId,
            cancellationToken);

        if (conversation is null)
            return Result<ConversationDto>.Failure("مکالمه مورد نظر پیدا نشد.");

        // فقط شرکت‌کنندگان می‌توانند مکالمه را ببینند
        if (!conversation.Participants.Any(participant => participant.UserId == userId))
            return Result<ConversationDto>.Failure("شما در این مکالمه عضو نیستید.");

        var dto = ConversationMapper.ToDto(conversation);
        dto.UnreadCount = await GetUnreadCountAsync(conversation, userId, cancellationToken);

        return Result<ConversationDto>.Success(dto);
    }

    /// <summary>علامت‌گذاری مکالمه به‌عنوان خوانده‌شده برای کاربر جاری.</summary>
    public async Task<Result> MarkAsReadAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetWithParticipantsAsync(
            conversationId,
            cancellationToken);

        if (conversation is null)
            return Result.Failure("مکالمه مورد نظر پیدا نشد.");

        var participant = conversation.Participants.FirstOrDefault(
            item => item.UserId == userId);

        if (participant is null)
            return Result.Failure("شما در این مکالمه عضو نیستید.");

        participant.MarkAsRead();
        _participantRepository.Update(participant);
        await _participantRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<MessageDto>>> GetMessagesAsync(
        int conversationId,
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var access = await EnsureParticipantAsync(conversationId, userId, cancellationToken);

        if (!access.IsSuccess)
            return Result<IReadOnlyList<MessageDto>>.Failure(access.Message ?? "دسترسی غیرمجاز.");

        var messages = await _messageRepository.GetByConversationAsync(
            conversationId,
            page,
            pageSize,
            cancellationToken);

        return Result<IReadOnlyList<MessageDto>>.Success(messages.Select(MessageMapper.ToDto).ToList());
    }

    public async Task<Result<MessageDto>> SendMessageAsync(
        int senderId,
        SendMessageDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result<MessageDto>.Failure("اطلاعات پیام الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.Content))
            return Result<MessageDto>.Failure("متن پیام الزامی است.");

        if (dto.ConversationId <= 0)
            return Result<MessageDto>.Failure("شناسه‌ی مکالمه معتبر نیست.");

        var conversation = await _conversationRepository.GetWithParticipantsAsync(
            dto.ConversationId,
            cancellationToken);

        if (conversation is null)
            return Result<MessageDto>.Failure("مکالمه مورد نظر پیدا نشد.");

        var sender = conversation.Participants.FirstOrDefault(participant =>
            participant.UserId == senderId);

        if (sender is null)
            return Result<MessageDto>.Failure("شما در این مکالمه عضو نیستید.");

        var message = new Message(conversation.Id, senderId, dto.Content.Trim());

        await _messageRepository.AddAsync(message, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        return Result<MessageDto>.Success(new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = UserMapper.DisplayName(sender.User),
            Content = message.Content,
            CreatedAt = message.CreatedAt
        });
    }

    /// <summary>شمارش پیام‌های خوانده‌نشده برای کاربر جاری.</summary>
    private async Task<int> GetUnreadCountAsync(
        Conversation conversation,
        int userId,
        CancellationToken cancellationToken)
    {
        var participant = conversation.Participants.FirstOrDefault(
            item => item.UserId == userId);

        return await _messageRepository.CountUnreadAsync(
            conversation.Id,
            userId,
            participant?.LastReadAt,
            cancellationToken);
    }

    private async Task<Conversation?> FindDirectAsync(
        int adminId,
        int userId,
        CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetForUserAsync(adminId, cancellationToken);

        return conversations.FirstOrDefault(conversation =>
            conversation.Type == ConversationType.AdminUser
            && conversation.Participants.Any(participant => participant.UserId == userId));
    }

    /// <summary>افزودن عضو بدون ذخیره‌سازی؛ ذخیره پس از همه‌ی اعضا انجام می‌شود.</summary>
    private async Task AddParticipantAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken)
        => await _participantRepository.AddIfMissingAsync(conversationId, userId, cancellationToken);

    private async Task<Result> EnsureParticipantAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetWithParticipantsAsync(
            conversationId,
            cancellationToken);

        if (conversation is null)
            return Result.Failure("مکالمه مورد نظر پیدا نشد.");

        return conversation.Participants.Any(participant => participant.UserId == userId)
            ? Result.Success()
            : Result.Failure("شما در این مکالمه عضو نیستید.");
    }

    private async Task<Result> ValidateAdminAsync(int adminId, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(adminId, cancellationToken);

        if (admin is null)
            return Result.Failure("ادمین مورد نظر پیدا نشد.");

        return admin.UserRole == UserRole.Admin
            ? Result.Success()
            : Result.Failure("فقط ادمین می‌تواند این عملیات را انجام دهد.");
    }
}