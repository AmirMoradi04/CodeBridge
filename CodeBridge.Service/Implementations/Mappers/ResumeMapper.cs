using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Resume;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class ResumeMapper
{
    public static ResumeDto ToDto(Resume resume) => new()
    {
        Id = resume.Id,
        UserId = resume.UserId,
        Title = resume.Title,
        Summary = resume.Summary,
        Skills = resume.Skills,
        AttachmentId = resume.AttachmentId,
        Status = resume.Status,
        ReviewedByAdminId = resume.ReviewedByAdminId,
        ReviewedAt = resume.ReviewedAt,
        CreatedAt = resume.CreatedAt
    };
}
