using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Contract;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class ContractMapper
{
    public static ContractDto ToDto(Contract contract) => new()
    {
        Id = contract.Id,
        ProjectId = contract.ProjectId,
        DeveloperId = contract.DeveloperId,
        EmployerId = contract.EmployerId,
        AdminId = contract.AdminId,
        TotalAmount = contract.TotalAmount,
        PlatformFee = contract.PlatformFee,
        DeveloperShare = contract.DeveloperShare,
        MentorShare = contract.MentorShare,
        DepositPaid = contract.DepositPaid,
        IsFullyPaid = contract.IsFullyPaid,
        Status = contract.Status,
        SignedAt = contract.SignedAt,
        IsNdaSigned = contract.IsNdaSigned,
        NdaSignedAt = contract.NdaSignedAt,
        CreatedAt = contract.CreatedAt
    };
}
