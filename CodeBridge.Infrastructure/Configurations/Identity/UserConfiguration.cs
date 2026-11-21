using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations.Identity;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(user => user.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.LastName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.Bio).HasMaxLength(1000);

        // ذخیره‌ی نقش به‌صورت رشته برای خوانایی
        builder.Property(user => user.UserRole)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(user => user.IsApproved).HasDefaultValue(false);
        builder.Property(user => user.IsActive).HasDefaultValue(true);

        // تصویر پروفایل (کلید خارجی مستقل از فایل‌های مالکیت کاربر)
        builder.HasOne(user => user.ProfilePicture)
            .WithMany()
            .HasForeignKey(user => user.ProfilePictureAttachmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // لوگوی شرکت (کلید خارجی مستقل از فایل‌های مالکیت کاربر)
        // نکته: SQL Server داشتن دو مسیر کسکید از Users به Attachments را ممنوع می‌کند،
        // بنابراین این رابطه (برخلاف تصویر پروفایل) با Restrict تنظیم شده است.
        builder.HasOne(user => user.CompanyLogo)
            .WithMany()
            .HasForeignKey(user => user.CompanyLogoAttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // رابطه‌ی یک‌به‌یک با رزومه
        builder.HasOne(user => user.Resume)
            .WithOne(resume => resume.User)
            .HasForeignKey<Resume>(resume => resume.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه‌ی یک‌به‌یک با سطح برنامه‌نویس
        builder.HasOne(user => user.DeveloperLevel)
            .WithOne(level => level.User)
            .HasForeignKey<DeveloperLevel>(level => level.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // کد معرف یکتاست؛ برای کاربران قدیمی مقدار NULL مجاز است
        builder.Property(user => user.ReferralCode).HasMaxLength(40);
        builder.HasIndex(user => user.ReferralCode).IsUnique();

        // کاربر دعوت‌کننده با حذف کاربر حذف نمی‌شود
        builder.HasOne(user => user.ReferredBy)
            .WithMany(referrer => referrer.Referrals)
            .HasForeignKey(user => user.ReferredById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
