using CodeBridge.Domain.Common;
using CodeBridge.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CodeBridge.Domain.Entities.Identity;

/// <summary>
/// کاربر پلتفرم بر پایه‌ی ASP.NET Core Identity.
/// از BaseEntity ارث نمی‌برد و فقط قرارداد IAuditableEntity را پیاده‌سازی می‌کند.
/// </summary>
public class User : IdentityUser<int>, IAuditableEntity
{
    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public UserRole UserRole { get; private set; }

    /// <summary>تایید حساب توسط ادمین.</summary>
    public bool IsApproved { get; private set; }

    public bool IsActive { get; private set; } = true;

    public string? Bio { get; private set; }

    public int? ProfilePictureAttachmentId { get; private set; }

    /// <summary>لوگوی شرکت کارفرما؛ فقط برای نقش کارفرما استفاده می‌شود.</summary>
    public int? CompanyLogoAttachmentId { get; private set; }

    // Id از IdentityUser<int> به ارث می‌رسد (setter آن در اختیار Identity است)
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    /// <summary>کد معرف یکتای کاربر برای دعوت دیگران (برای کاربران قدیمی ممکن است خالی باشد).</summary>
    public string? ReferralCode { get; private set; }

    /// <summary>کاربری که با کد معرف او ثبت‌نام شده است.</summary>
    public int? ReferredById { get; private set; }

    // --- Navigation Properties ---
    public Attachment? ProfilePicture { get; private set; }

    public Attachment? CompanyLogo { get; private set; }

    public Resume? Resume { get; private set; }

    public DeveloperLevel? DeveloperLevel { get; private set; }

    public ICollection<Attachment> Attachments { get; private set; } = new List<Attachment>();

    public ICollection<Project> EmployerProjects { get; private set; } = new List<Project>();

    public ICollection<Project> AssignedProjects { get; private set; } = new List<Project>();

    public ICollection<Post> Posts { get; private set; } = new List<Post>();

    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    public ICollection<Like> Likes { get; private set; } = new List<Like>();

    public ICollection<Contract> DeveloperContracts { get; private set; } = new List<Contract>();

    public ICollection<Contract> EmployerContracts { get; private set; } = new List<Contract>();

    public ICollection<Contract> AdminContracts { get; private set; } = new List<Contract>();

    public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public ICollection<ProjectMember> ProjectMemberships { get; private set; } = new List<ProjectMember>();

    public ICollection<ConversationParticipant> ConversationParticipations { get; private set; }
        = new List<ConversationParticipant>();

    public ICollection<Message> SentMessages { get; private set; } = new List<Message>();

    public ICollection<DailyProject> DailyProjects { get; private set; } = new List<DailyProject>();

    public ICollection<DailyProjectApplication> DailyApplications { get; private set; }
        = new List<DailyProjectApplication>();

    /// <summary>جلسه‌های زنده‌ای که این کاربر به‌عنوان برنامه‌نویس ارشد برگزار می‌کند.</summary>
    public ICollection<LiveSession> HostedLiveSessions { get; private set; }
        = new List<LiveSession>();

    /// <summary>جلسه‌های زنده‌ای که این کاربر به‌عنوان کارجو در آن‌ها شرکت کرده است.</summary>
    public ICollection<LiveSessionParticipant> LiveSessionParticipations { get; private set; }
        = new List<LiveSessionParticipant>();

    public Wallet? Wallet { get; private set; }

    public User? ReferredBy { get; private set; }

    public ICollection<User> Referrals { get; private set; } = new List<User>();

    // سازنده‌ی بدون پارامتر برای Identity و EF Core
    public User()
    {
    }

    public User(string firstName, string lastName, string email, UserRole userRole) : base(email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserRole = userRole;
        IsApproved = false;
        IsActive = true;
        LockoutEnabled = true;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    /// <summary>اعتبارسنجی اطلاعات پایه‌ی کاربر.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            throw new InvalidOperationException("نام کاربر الزامی است.");

        if (string.IsNullOrWhiteSpace(LastName))
            throw new InvalidOperationException("نام خانوادگی کاربر الزامی است.");

        if (string.IsNullOrWhiteSpace(Email))
            throw new InvalidOperationException("ایمیل کاربر الزامی است.");
    }

    /// <summary>تایید حساب توسط ادمین.</summary>
    public void Approve()
    {
        IsApproved = true;
        SetUpdatedAt();
    }

    /// <summary>رد یا لغو تایید حساب.</summary>
    public void Reject()
    {
        IsApproved = false;
        SetUpdatedAt();
    }

    /// <summary>آیا حساب در لحظه‌ی داده‌شده به دلیل تلاش‌های ناموفق قفل است.</summary>
    public bool IsLockedOut(DateTime utcNow)
        => LockoutEnd.HasValue && LockoutEnd.Value.UtcDateTime > utcNow;

    /// <summary>
    /// ثبت یک تلاش ناموفق ورود؛ با رسیدن به سقف، حساب برای مدت مشخص قفل می‌شود
    /// و شمارنده صفر می‌شود تا پس از پایان قفل، فرصت‌های تازه شروع شود.
    /// </summary>
    public void RegisterFailedLogin(int maxAttempts, TimeSpan lockoutDuration, DateTime utcNow)
    {
        if (maxAttempts <= 0)
            throw new InvalidOperationException("سقف تلاش ناموفق باید بزرگ‌تر از صفر باشد.");

        if (lockoutDuration <= TimeSpan.Zero)
            throw new InvalidOperationException("مدت قفل حساب باید بزرگ‌تر از صفر باشد.");

        AccessFailedCount += 1;

        if (AccessFailedCount >= maxAttempts)
        {
            LockoutEnd = new DateTimeOffset(utcNow.Add(lockoutDuration), TimeSpan.Zero);
            AccessFailedCount = 0;
        }

        SetUpdatedAt();
    }

    /// <summary>پاک‌کردن شمارنده و قفل پس از ورود موفق.</summary>
    public void ResetFailedLogin()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        SetUpdatedAt();
    }

    /// <summary>به‌روزرسانی اطلاعات پروفایل.</summary>
    public void UpdateProfile(
        string firstName,
        string lastName,
        string? bio,
        bool isActive,
        int? profilePictureAttachmentId = null,
        int? companyLogoAttachmentId = null)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Bio = bio;
        IsActive = isActive;
        ProfilePictureAttachmentId = profilePictureAttachmentId;
        CompanyLogoAttachmentId = companyLogoAttachmentId;

        Validate();
        SetUpdatedAt();
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        SetUpdatedAt();
    }

    /// <summary>ثبت کد معرف تولیدشده برای کاربر؛ فقط یک بار تنظیم می‌شود.</summary>
    public void SetReferralCode(string referralCode)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
            throw new InvalidOperationException("کد معرف نمی‌تواند خالی باشد.");

        if (!string.IsNullOrWhiteSpace(ReferralCode))
            return; // کد قبلاً ثبت شده و نباید بازنویسی شود

        ReferralCode = referralCode.Trim();
        SetUpdatedAt();
    }

    /// <summary>ثبت کاربر دعوت‌کننده برای کاربر تازه.</summary>
    public void ApplyReferral(int referrerId)
    {
        if (referrerId == Id)
            throw new InvalidOperationException("کاربر نمی‌تواند خودش را معرفی کند.");

        if (ReferredById.HasValue)
            throw new InvalidOperationException("کد معرف قبلاً برای این حساب اعمال شده است.");

        ReferredById = referrerId;
        SetUpdatedAt();
    }

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
