using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.DailyProject;

/// <summary>پروژه‌ی روزمره.</summary>
public sealed class DailyProjectDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int EmployerId { get; init; }
    public string? EmployerName { get; init; }
    public decimal TotalBudget { get; init; }
    public decimal PricePerUnit { get; init; }
    public int Capacity { get; init; }
    public int FilledSlots { get; init; }
    public int DeadlineSeconds { get; init; }
    public DailyProjectStatus Status { get; init; }
    public bool ProofRequired { get; init; }
    public string? Instructions { get; init; }
    public string? AdminNote { get; init; }
    public decimal PaidAmount { get; init; }
    public bool IsPaid { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}