using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Project;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class ProjectMapper
{
    public static ProjectDto ToDto(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Description = project.Description,
        Budget = project.Budget,
        EmployerId = project.EmployerId,
        EmployerName = project.Employer is null ? null : UserMapper.DisplayName(project.Employer),
        AssignedDeveloperId = project.AssignedDeveloperId,
        AssignedDeveloperName = project.AssignedDeveloper is null
            ? null
            : UserMapper.DisplayName(project.AssignedDeveloper),
        RequiredLevel = project.RequiredLevel,
        Status = project.Status,
        AdminNote = project.AdminNote,
        IsPublic = project.IsPublic,
        PublishedAt = project.PublishedAt,
        IsFinalApproved = project.IsFinalApproved,
        FinalApprovedAt = project.FinalApprovedAt,
        CreatedAt = project.CreatedAt
    };
}
