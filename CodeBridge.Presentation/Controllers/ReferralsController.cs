using CodeBridge.Service.DTOs.Referral;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>پنل معرف: کد معرف، دعوت‌شدگان و پاداش‌ها.</summary>
[Route("api/[controller]")]
[Authorize]
public class ReferralsController : BaseApiController
{
    private readonly IReferralService _referralService;

    public ReferralsController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    /// <summary>اطلاعات معرف برای کاربر جاری.</summary>
    [HttpGet("my-info")]
    public async Task<ActionResult<ReferralInfoDto>> GetMyInfo(CancellationToken cancellationToken)
        => Handle(await _referralService.GetMyReferralInfoAsync(CurrentUserId(), cancellationToken));
}
