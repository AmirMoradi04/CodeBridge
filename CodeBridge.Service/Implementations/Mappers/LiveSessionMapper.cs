using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.LiveSession;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class LiveSessionMapper
{
    /// <summary>تبدیل جلسه به DTO؛ لینک‌ها فقط در صورت مجاز بودن پر می‌شوند.</summary>
    public static LiveSessionDto ToDto(
        LiveSession session,
        bool isJoined = false,
        bool includeLinks = false)
    {
        var participantCount = session.Participants?.Count ?? 0;

        return new LiveSessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            ProjectTitle = session.Project?.Title,
            SeniorId = session.SeniorId,
            SeniorName = session.Senior is null
                ? null
                : UserMapper.DisplayName(session.Senior),
            Title = session.Title,
            Description = session.Description,
            Price = session.Price,
            ScheduledAt = session.ScheduledAt,
            DurationMinutes = session.DurationMinutes,
            MeetingLink = includeLinks ? session.MeetingLink : null,
            RecordingLink = includeLinks ? session.RecordingLink : null,
            Status = session.Status,
            ParticipantCount = participantCount,
            TotalRevenue = session.Price * participantCount,
            IsJoined = isJoined,
            CreatedAt = session.CreatedAt
        };
    }
}
