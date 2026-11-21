using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.DailyProject;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد عملیات پروژه‌های روزمره و تسک‌ها.</summary>
public interface IDailyProjectService
{
    // --- CRUD پروژه ---
    Task<Result<DailyProjectDto>> CreateAsync(int employerId, CreateDailyProjectDto dto, CancellationToken cancellationToken = default);
    Task<Result<DailyProjectDto>> GetByIdAsync(int projectId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DailyProjectDto>>> GetByEmployerAsync(int employerId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DailyProjectDto>>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DailyProjectDto>>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DailyProjectDto>>> GetApprovedAsync(CancellationToken cancellationToken = default);

    // --- مدیریت درخواست (ادمین) ---
    Task<Result> ApproveAsync(int projectId, int adminId, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(int projectId, int adminId, string note, CancellationToken cancellationToken = default);
    Task<Result> ActivateAsync(int projectId, int adminId, CancellationToken cancellationToken = default);

    // --- پرداخت بودجه توسط کارفرما ---
    Task<Result> DepositFundsAsync(int projectId, int employerId, CancellationToken cancellationToken = default);

    // --- عملیات توسعه‌دهنده ---
    Task<Result<DailyApplicationDto>> ApplyAsync(int developerId, int projectId, CancellationToken cancellationToken = default);
    Task<Result> StartTaskAsync(int developerId, StartTaskDto dto, CancellationToken cancellationToken = default);
    Task<Result> SubmitTaskAsync(int developerId, SubmitTaskDto dto, CancellationToken cancellationToken = default);

    // --- بررسی ادمین روی تسک ---
    Task<Result> ApproveTaskAsync(int applicationId, int adminId, CancellationToken cancellationToken = default);
    Task<Result> RejectTaskAsync(int applicationId, int adminId, string note, CancellationToken cancellationToken = default);
    Task<Result> ExpireTaskAsync(int applicationId, int adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// منقضی‌کردن خودکار همه‌ی درخواست‌های در حال انجامی که مهلتشان گذشته است.
    /// توسط سرویس پس‌زمینه (Hangfire) هر دقیقه فراخوانی می‌شود.
    /// </summary>
    Task<Result> ExpireStaleApplicationsAsync(CancellationToken cancellationToken = default);

    // --- درخواست‌ها ---
    Task<Result<IReadOnlyList<DailyApplicationDto>>> GetApplicationsByProjectAsync(int projectId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DailyApplicationDto>>> GetApplicationsByDeveloperAsync(int developerId, CancellationToken cancellationToken = default);
}