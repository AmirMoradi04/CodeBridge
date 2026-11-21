namespace CodeBridge.Service.DTOs.DailyProject;

/// <summary>ایجاد پروژه‌ی روزمره توسط کارفرما.</summary>
public sealed class CreateDailyProjectDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal TotalBudget { get; init; }
    public decimal PricePerUnit { get; init; }
    public int Capacity { get; init; }
    public int DeadlineSeconds { get; init; } = 60;
    public string? Instructions { get; init; }
}