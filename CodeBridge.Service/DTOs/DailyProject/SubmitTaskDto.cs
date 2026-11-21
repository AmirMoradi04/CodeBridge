namespace CodeBridge.Service.DTOs.DailyProject;

/// <summary>ارسال اثبات تسک توسط توسعه‌دهنده.</summary>
public sealed class SubmitTaskDto
{
    public int DailyProjectId { get; init; }
    public int ProofAttachmentId { get; init; }
}