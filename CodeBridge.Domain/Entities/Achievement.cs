using CodeBridge.Domain.Common;
using CodeBridge.Domain.Enums;

namespace CodeBridge.Domain.Entities;

/// <summary>تعریف یک دستاورد قابل کسب؛ داده‌ی پایه که در DbInitializer کاشت می‌شود.</summary>
public class Achievement : BaseEntity
{
    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? IconUrl { get; private set; }

    public AchievementType Type { get; private set; }

    // سازنده‌ی بدون پارامتر برای EF Core
    private Achievement()
    {
    }

    public Achievement(string title, string description, AchievementType type, string? iconUrl = null)
    {
        Title = title;
        Description = description;
        Type = type;
        IconUrl = iconUrl;

        Validate();
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("عنوان دستاورد الزامی است.");

        if (string.IsNullOrWhiteSpace(Description))
            throw new InvalidOperationException("توضیحات دستاورد الزامی است.");
    }
}
