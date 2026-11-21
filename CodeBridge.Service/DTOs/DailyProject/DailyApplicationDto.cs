using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.DailyProject;

/// <summary>درخواست توسعه‌دهنده برای انجام تسک روزمره.</summary>
public sealed class DailyApplicationDto
{
    public int Id { get; init; }
    public int DailyProjectId { get; init; }
    public int DeveloperId { get; init; }
    public string? DeveloperName { get; init; }
    public DailyApplicationStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public int? ProofAttachmentId { get; init; }
    public string? AdminNote { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}