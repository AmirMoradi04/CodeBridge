namespace CodeBridge.Service.DTOs.Referral;

/// <summary>اطلاعات پنل معرف کاربر جاری.</summary>
public sealed class ReferralInfoDto
{
    public string? ReferralCode { get; init; }

    /// <summary>مبلغ پاداش هر دعوت موفق.</summary>
    public decimal RewardAmount { get; init; }

    /// <summary>مجموع پاداش دریافت‌شده توسط کاربر.</summary>
    public decimal TotalEarned { get; init; }

    public IReadOnlyList<ReferredUserDto> ReferredUsers { get; init; } = Array.Empty<ReferredUserDto>();
}

/// <summary>کاربری که با کد معرف دعوت شده است.</summary>
public sealed class ReferredUserDto
{
    public int UserId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public bool IsApproved { get; init; }

    public DateTime JoinedAt { get; init; }
}
