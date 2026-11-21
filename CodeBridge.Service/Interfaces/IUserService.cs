using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.User;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد پروفایل و فهرست کاربران.</summary>
public interface IUserService
{
    Task<Result<UserProfileDto>> GetProfileAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result> UpdateProfileAsync(
        int userId,
        UpdateProfileDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(
        UserRole? role = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<UserDto>>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<UserDto>>> GetJobSeekersByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default);

    Task<Result<int>> CountByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}
