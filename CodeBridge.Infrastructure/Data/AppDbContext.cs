using CodeBridge.Domain.Common;
using CodeBridge.Domain.Entities;
using CodeBridge.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeBridge.Infrastructure.Data;

/// <summary>
/// زمینه‌ی داده‌ی اصلی روی SQL Server با پشتیبانی از ASP.NET Core Identity.
/// رشته‌ی اتصال از طریق constructor و از appsettings پروژه‌ی Presentation تزریق می‌شود.
/// </summary>
public class AppDbContext : IdentityDbContext<User, Role, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Resume> Resumes => Set<Resume>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Like> Likes => Set<Like>();

    public DbSet<Contract> Contracts => Set<Contract>();

    public DbSet<DeveloperLevel> DeveloperLevels => Set<DeveloperLevel>();

    public DbSet<Attachment> Attachments => Set<Attachment>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    public DbSet<DailyProject> DailyProjects => Set<DailyProject>();

    public DbSet<DailyProjectApplication> DailyProjectApplications => Set<DailyProjectApplication>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<ReferralReward> ReferralRewards => Set<ReferralReward>();

    public DbSet<Achievement> Achievements => Set<Achievement>();

    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    public DbSet<LiveSession> LiveSessions => Set<LiveSession>();

    public DbSet<LiveSessionParticipant> LiveSessionParticipants => Set<LiveSessionParticipant>();

    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // کانفیگ Identity از IdentityDbContext اعمال شد؛ حالا کانفیگ‌های اختصاصی پروژه
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // ثبت خودکار زمان ایجاد
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = utcNow;
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                // ثبت خودکار زمان بروزرسانی و جلوگیری از تغییر CreatedAt/Id
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = utcNow;
                entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.Id)).IsModified = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
