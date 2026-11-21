using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.DailyProject;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class DailyProjectMapper
{
    public static DailyProjectDto ToDto(DailyProject project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Description = project.Description,
        EmployerId = project.EmployerId,
        EmployerName = project.Employer is null ? null : UserMapper.DisplayName(project.Employer),
        TotalBudget = project.TotalBudget,
        PricePerUnit = project.PricePerUnit,
        Capacity = project.Capacity,
        FilledSlots = project.FilledSlots,
        DeadlineSeconds = project.DeadlineSeconds,
        Status = project.Status,
        ProofRequired = project.ProofRequired,
        Instructions = project.Instructions,
        AdminNote = project.AdminNote,
        PaidAmount = project.PaidAmount,
        IsPaid = project.IsPaid,
        ApprovedAt = project.ApprovedAt,
        CreatedAt = project.CreatedAt
    };

    public static DailyApplicationDto ToApplicationDto(DailyProjectApplication app) => new()
    {
        Id = app.Id,
        DailyProjectId = app.DailyProjectId,
        DeveloperId = app.DeveloperId,
        DeveloperName = app.Developer is null ? null : UserMapper.DisplayName(app.Developer),
        Status = app.Status,
        StartedAt = app.StartedAt,
        ExpiresAt = app.ExpiresAt,
        SubmittedAt = app.SubmittedAt,
        ProofAttachmentId = app.ProofAttachmentId,
        AdminNote = app.AdminNote,
        ReviewedAt = app.ReviewedAt,
        CreatedAt = app.CreatedAt
    };
}