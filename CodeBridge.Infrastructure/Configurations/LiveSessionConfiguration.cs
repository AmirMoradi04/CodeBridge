using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class LiveSessionConfiguration : BaseConfiguration<LiveSession>
{
    public override void Configure(EntityTypeBuilder<LiveSession> builder)
    {
        base.Configure(builder);

        builder.ToTable("LiveSessions");

        builder.Property(session => session.Title).IsRequired().HasMaxLength(200);
        builder.Property(session => session.Description).HasMaxLength(2000);
        builder.Property(session => session.Price).HasPrecision(18, 2);
        builder.Property(session => session.DurationMinutes).IsRequired();

        // لینک‌های جلسه و ضبط، آدرس‌های بیرونی هستند و در دیتابیس فقط متن ذخیره می‌شود
        builder.Property(session => session.MeetingLink).IsRequired().HasMaxLength(500);
        builder.Property(session => session.RecordingLink).HasMaxLength(500);

        builder.Property(session => session.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(session => session.SeniorId);
        builder.HasIndex(session => session.ProjectId);
        builder.HasIndex(session => session.Status);

        builder.HasOne(session => session.Project)
            .WithMany()
            .HasForeignKey(session => session.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(session => session.Senior)
            .WithMany(user => user.HostedLiveSessions)
            .HasForeignKey(session => session.SeniorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
