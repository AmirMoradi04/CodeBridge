using CodeBridge.Domain.Entities.Identity;
using CodeBridge.Service.DTOs.DeveloperLevel;
using CodeBridge.Service.DTOs.Resume;
using CodeBridge.Service.DTOs.User;

namespace CodeBridge.Service.Implementations.Mappers;

/// <summary>تبدیل دستی انتیتی کاربر به DTO.</summary>
internal static class UserMapper
{
    public static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty,
        UserRole = user.UserRole,
        IsApproved = user.IsApproved,
        IsActive = user.IsActive,
        Bio = user.Bio,
        ProfilePictureUrl = AttachmentUrl(user.ProfilePictureAttachmentId),
        CompanyLogoUrl = AttachmentUrl(user.CompanyLogoAttachmentId),
        Level = user.DeveloperLevel?.Level,
        Score = user.DeveloperLevel?.Score,
        CreatedAt = user.CreatedAt
    };

    public static UserProfileDto ToProfileDto(
        User user,
        ResumeDto? resume = null,
        DeveloperLevelDto? developerLevel = null) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty,
        UserRole = user.UserRole,
        IsApproved = user.IsApproved,
        IsActive = user.IsActive,
        Bio = user.Bio,
        ProfilePictureUrl = AttachmentUrl(user.ProfilePictureAttachmentId),
        CompanyLogoUrl = AttachmentUrl(user.CompanyLogoAttachmentId),
        CreatedAt = user.CreatedAt,
        Resume = resume,
        DeveloperLevel = developerLevel
    };

    /// <summary>
    /// آدرس دانلود فایل پیوست از طریق endpoint احراز هویت‌شده‌ی پیوست‌ها.
    /// فایل‌ها فقط از همین مسیر قابل دسترسی‌اند و لینک عمومی ندارند.
    /// </summary>
    private static string? AttachmentUrl(int? attachmentId)
        => attachmentId.HasValue ? $"/api/attachments/{attachmentId.Value}/file" : null;

    public static string DisplayName(User? user)
        => user is null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim();
}
