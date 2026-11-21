using CodeBridge.Domain.Enums;
using CodeBridge.Service.Common;
using CodeBridge.Service.DTOs.Contract;

namespace CodeBridge.Service.Interfaces;

/// <summary>قرارداد ساخت، امضا، NDA و تکمیل قرارداد.</summary>
public interface IContractService
{
    Task<Result<ContractDto>> CreateAsync(
        CreateContractDto dto,
        int adminId,
        CancellationToken cancellationToken = default);

    /// <summary>خواندن قرارداد با بررسی دسترسی طرفین قرارداد در خود سرویس.</summary>
    Task<Result<ContractDto>> GetByIdAsync(
        int contractId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default);

    Task<Result<ContractDto?>> GetByProjectAsync(
        int projectId,
        int callerId,
        UserRole callerRole,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ContractDto>>> GetByDeveloperAsync(
        int developerId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ContractDto>>> GetByEmployerAsync(
        int employerId,
        CancellationToken cancellationToken = default);

    Task<Result> SignAsync(int contractId, int userId, CancellationToken cancellationToken = default);

    /// <summary>امضای توافق‌نامه‌ی عدم افشا توسط برنامه‌نویس طرف قرارداد.</summary>
    Task<Result> SignNdaAsync(
        int contractId,
        int developerId,
        CancellationToken cancellationToken = default);

    Task<Result> CompleteAsync(int contractId, int adminId, CancellationToken cancellationToken = default);
}