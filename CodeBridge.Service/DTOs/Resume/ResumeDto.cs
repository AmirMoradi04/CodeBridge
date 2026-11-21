using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Resume;

public sealed class ResumeDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string Skills { get; init; } = string.Empty;

    public int? AttachmentId { get; init; }

    public ResumeStatus Status { get; init; }

    public int? ReviewedByAdminId { get; init; }

    public DateTime? ReviewedAt { get; init; }

    public DateTime CreatedAt { get; init; }
}
