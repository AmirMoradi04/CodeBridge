using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Resume;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد مدیریت رزومه و فرایند بررسی.</summary>
public interface IResumeService
{
    Task<Result<ResumeDto>> CreateOrUpdateAsync(
        int userId,
        CreateResumeDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<ResumeDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ResumeDto>>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ResumeDto>>> GetApprovedAsync(CancellationToken cancellationToken = default);

    Task<Result> ApproveAsync(int resumeId, int adminId, CancellationToken cancellationToken = default);

    Task<Result> RejectAsync(int resumeId, int adminId, CancellationToken cancellationToken = default);
}
