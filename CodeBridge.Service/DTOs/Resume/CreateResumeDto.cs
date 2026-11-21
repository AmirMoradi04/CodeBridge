namespace CodeBridge.Service.DTOs.Resume;

public sealed class CreateResumeDto
{
    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string Skills { get; init; } = string.Empty;

    public int? AttachmentId { get; init; }
}
