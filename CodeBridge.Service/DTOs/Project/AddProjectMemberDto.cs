namespace CodeBridge.Service.DTOs.Project;

/// <summary>افزودن عضو به پروژه؛ ProjectId باید با شناسه‌ی مسیر یکی باشد.</summary>
public sealed class AddProjectMemberDto
{
    public int ProjectId { get; init; }

    public int UserId { get; init; }
}