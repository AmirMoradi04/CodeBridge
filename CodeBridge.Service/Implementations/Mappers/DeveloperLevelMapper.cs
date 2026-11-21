using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.DeveloperLevel;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class DeveloperLevelMapper
{
    public static DeveloperLevelDto ToDto(DeveloperLevel level) => new()
    {
        Id = level.Id,
        UserId = level.UserId,
        Level = level.Level,
        Score = level.Score,
        ProjectsCompleted = level.ProjectsCompleted,
        AverageRating = level.AverageRating
    };
}
