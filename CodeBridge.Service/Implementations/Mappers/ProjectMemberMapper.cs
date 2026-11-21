using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Project;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class ProjectMemberMapper
{
    public static ProjectMemberDto ToDto(ProjectMember member) => new()
    {
        Id = member.Id,
        ProjectId = member.ProjectId,
        UserId = member.UserId,
        UserName = UserMapper.DisplayName(member.User),
        UserEmail = member.User?.Email ?? string.Empty,
        UserRole = member.User?.UserRole ?? default,
        IsApproved = member.User?.IsApproved ?? false,
        JoinedAt = member.JoinedAt
    };
}