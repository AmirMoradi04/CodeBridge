namespace CodeBridge.Service.DTOs.Auth;

/// <summary>فایل رزومه‌ی ارسالی همراه با ثبت‌نام کارجو.</summary>
public sealed record ResumeUpload(Stream Content, string FileName, string ContentType);
