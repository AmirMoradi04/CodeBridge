namespace CodeBridge.Service.DTOs.Contract;

public sealed class CreateContractDto
{
    public int ProjectId { get; init; }

    public decimal TotalAmount { get; init; }

    public decimal PlatformFee { get; init; }

    public decimal DeveloperShare { get; init; }

    public decimal? MentorShare { get; init; }

    public int? MentorId { get; init; }
}
