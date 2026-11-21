using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Enums;
using CodeBridge.Domain.Interfaces.Repositories;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.User;
using CodeBridge.Service.Implementations.Mappers;
using CodeBridge.Service.Interfaces;

namespace CodeBridge.Service.Implementations;

/// <summary>مدیریت پروفایل و فهرست کاربران.</summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAttachmentRepository _attachmentRepository;

    public UserService(IUserRepository userRepository, IAttachmentRepository attachmentRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _attachmentRepository = attachmentRepository
            ?? throw new ArgumentNullException(nameof(attachmentRepository));
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithResumeAndLevelAsync(userId, cancellationToken);

        if (user is null)
            return Result<UserProfileDto>.Failure("کاربر مورد نظر پیدا نشد.");

        var resume = user.Resume is null ? null : ResumeMapper.ToDto(user.Resume);
        var level = user.DeveloperLevel is null ? null : DeveloperLevelMapper.ToDto(user.DeveloperLevel);

        return Result<UserProfileDto>.Success(UserMapper.ToProfileDto(user, resume, level));
    }

    public async Task<Result> UpdateProfileAsync(
        int userId,
        UpdateProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
            return Result.Failure("اطلاعات پروفایل الزامی است.");

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return Result.Failure("نام و نام خانوادگی الزامی است.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure("کاربر مورد نظر پیدا نشد.");

        // فایل‌های پیوست فقط باید متعلق به خود کاربر باشند
        var profilePictureId = await ValidateOwnedAttachmentAsync(
            dto.ProfilePictureAttachmentId,
            userId,
            cancellationToken);

        if (dto.ProfilePictureAttachmentId.HasValue && profilePictureId is null)
            return Result.Failure("فایل آواتار یا متعلق به شما نیست یا یافت نشد.");

        var companyLogoId = await ValidateOwnedAttachmentAsync(
            dto.CompanyLogoAttachmentId,
            userId,
            cancellationToken);

        if (dto.CompanyLogoAttachmentId.HasValue && companyLogoId is null)
            return Result.Failure("فایل لوگو یا متعلق به شما نیست یا یافت نشد.");

        user.UpdateProfile(
            dto.FirstName,
            dto.LastName,
            dto.Bio,
            user.IsActive,
            profilePictureId,
            companyLogoId);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// بررسی اینکه فایل پیوست متعلق به کاربر است؛ در غیر این صورت null برمی‌گرداند.
    /// </summary>
    private async Task<int?> ValidateOwnedAttachmentAsync(
        int? attachmentId,
        int userId,
        CancellationToken cancellationToken)
    {
        if (!attachmentId.HasValue)
            return null;

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId.Value, cancellationToken);

        if (attachment is null || attachment.OwnerId != userId)
            return null;

        return attachment.Id;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(
        UserRole? role = null,
        CancellationToken cancellationToken = default)
    {
        var users = role.HasValue
            ? await _userRepository.GetByRoleAsync(role.Value, cancellationToken)
            : await _userRepository.GetAllWithLevelsAsync(cancellationToken);

        return Result<IReadOnlyList<UserDto>>.Success(users.Select(UserMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetPendingApprovalsAsync(cancellationToken);

        return Result<IReadOnlyList<UserDto>>.Success(users.Select(UserMapper.ToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetJobSeekersByLevelAsync(
        DeveloperLevelType level,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetJobSeekersByLevelAsync(level, cancellationToken);

        return Result<IReadOnlyList<UserDto>>.Success(users.Select(UserMapper.ToDto).ToList());
    }

    public async Task<Result<int>> CountByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var count = await _userRepository.CountByRoleAsync(role, cancellationToken);

        return Result<int>.Success(count);
    }
}
