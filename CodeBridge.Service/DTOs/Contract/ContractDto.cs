using CodeBridge.Domain.Enums;

namespace CodeBridge.Service.DTOs.Contract;

public sealed class ContractDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public int DeveloperId { get; init; }

    public int EmployerId { get; init; }

    public int AdminId { get; init; }

    public decimal TotalAmount { get; init; }

    public decimal PlatformFee { get; init; }

    public decimal DeveloperShare { get; init; }

    public decimal? MentorShare { get; init; }

    public decimal DepositPaid { get; init; }

    public bool IsFullyPaid { get; init; }

    public ContractStatus Status { get; init; }

    public DateTime? SignedAt { get; init; }

    /// <summary>وضعیت امضای توافق‌نامه‌ی عدم افشا.</summary>
    public bool IsNdaSigned { get; init; }

    public DateTime? NdaSignedAt { get; init; }

    public DateTime CreatedAt { get; init; }
}
