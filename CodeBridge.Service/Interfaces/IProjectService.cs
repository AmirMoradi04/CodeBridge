using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Project;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد ثبت، بررسی، انتشار و تخصیص پروژه.</summary>
public interface IProjectService
{
    Task<Result<ProjectDto>> CreateAsync(
        int employerId,
        CreateProjectDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>خواندن پروژه با بررسی دسترسی صاحب‌کار/عضو/ادمین در خود سرویس.</summary>
    Task<Result<ProjectDto>> GetByIdAsync(
        int projectId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ProjectDto>>> GetByEmployerAsync(
        int employerId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ProjectDto>>> GetByDeveloperAsync(
        int developerId,
        CancellationToken cancellationToken = default);

    /// <summary>پروژه‌هایی که برنامه‌نویس در آنها تخصیص یافته یا عضو تیم است.</summary>
    Task<Result<IReadOnlyList<ProjectDto>>> GetAssignedAsync(
        int developerId,
        CancellationToken cancellationToken = default);

    /// <summary>همه‌ی پروژه‌ها برای ادمین با فیلتر اختیاری وضعیت.</summary>
    Task<Result<IReadOnlyList<ProjectDto>>> GetAllAsync(
        ProjectStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ProjectDto>>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ProjectDto>>> GetUnassignedAsync(CancellationToken cancellationToken = default);

    Task<Result> ApproveAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default);

    Task<Result> RejectAsync(
        int projectId,
        int adminId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result> AssignToDeveloperAsync(
        int projectId,
        AssignProjectDto dto,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>انتشار عمومی پروژه‌ی تاییدشده.</summary>
    Task<Result> PublishAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>واریز بودجه‌ی پروژه توسط کارفرما به حساب امانی (ESCROW).</summary>
    Task<Result> EmployerDepositAsync(
        int projectId,
        int employerId,
        decimal amount,
        CancellationToken cancellationToken = default);

    /// <summary>تایید نهایی پروژه و پرداخت به برنامه‌نویس/منتور از حساب امانی. فقط ادمین.</summary>
    Task<Result> FinalApproveAsync(
        int projectId,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>افزودن عضو به تیم پروژه توسط ادمین.</summary>
    Task<Result<ProjectMemberDto>> AddMemberAsync(
        AddProjectMemberDto dto,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>فهرست اعضای پروژه؛ فقط ادمین یا اعضای پروژه مجازند.</summary>
    Task<Result<IReadOnlyList<ProjectMemberDto>>> GetMembersAsync(
        int projectId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default);

    /// <summary>حذف عضو از تیم پروژه توسط ادمین.</summary>
    Task<Result> RemoveMemberAsync(
        int projectId,
        int userId,
        int adminId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateStatusAsync(
        int projectId,
        ProjectStatus status,
        CancellationToken cancellationToken = default);
}