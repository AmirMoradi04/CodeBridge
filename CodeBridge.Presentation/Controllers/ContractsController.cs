using CodeBridge.Domain.Enums;
using CodeBridge.Service.DTOs.Contract;
using CodeBridge.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBridge.Presentation.Controllers;

/// <summary>مدیریت قراردادها؛ همه‌ی منطق در IContractService است.</summary>
[Route("api/[controller]")]
public class ContractsController : BaseApiController
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    /// <summary>ساخت قرارداد فقط توسط ادمین.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ContractDto>> Create(
        [FromBody] CreateContractDto dto,
        CancellationToken cancellationToken)
        => Handle(await _contractService.CreateAsync(dto, CurrentUserId(), cancellationToken));

    /// <summary>جزئیات قرارداد؛ کنترل طرفین در سرویس انجام می‌شود.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<ContractDto>> GetById(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _contractService.GetByIdAsync(id, CurrentUserId(), CurrentUserRole(), cancellationToken));

    /// <summary>قرارداد مربوط به پروژه، فقط برای طرفین یا ادمین.</summary>
    [HttpGet("project/{projectId:int}")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<ActionResult<ContractDto?>> GetByProject(
        int projectId,
        CancellationToken cancellationToken)
        => Handle(await _contractService.GetByProjectAsync(
            projectId,
            CurrentUserId(),
            CurrentUserRole(),
            cancellationToken));

    [HttpGet("developer/{developerId:int}")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<ActionResult<IReadOnlyList<ContractDto>>> GetByDeveloper(
        int developerId,
        CancellationToken cancellationToken)
    {
        // هر برنامه‌نویس فقط قراردادهای خودش را می‌بیند
        if (developerId != CurrentUserId())
            return Forbid();

        return Handle(await _contractService.GetByDeveloperAsync(developerId, cancellationToken));
    }

    [HttpGet("employer/{employerId:int}")]
    [Authorize(Roles = nameof(UserRole.Employer))]
    public async Task<ActionResult<IReadOnlyList<ContractDto>>> GetByEmployer(
        int employerId,
        CancellationToken cancellationToken)
    {
        // هر کارفرما فقط قراردادهای خودش را می‌بیند
        if (employerId != CurrentUserId())
            return Forbid();

        return Handle(await _contractService.GetByEmployerAsync(employerId, cancellationToken));
    }

    [HttpPatch("{id:int}/sign")]
    [Authorize(Roles = "Admin,Employer,JobSeeker")]
    public async Task<IActionResult> Sign(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _contractService.SignAsync(id, CurrentUserId(), cancellationToken));

    /// <summary>امضای توافق‌نامه‌ی عدم افشا فقط توسط برنامه‌نویس طرف قرارداد.</summary>
    [HttpPost("{id:int}/sign-nda")]
    [Authorize(Roles = nameof(UserRole.JobSeeker))]
    public async Task<IActionResult> SignNda(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _contractService.SignNdaAsync(id, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/complete")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Complete(
        int id,
        CancellationToken cancellationToken)
        => Handle(await _contractService.CompleteAsync(id, CurrentUserId(), cancellationToken));
}