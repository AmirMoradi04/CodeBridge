using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class LiveSessionParticipantConfiguration
    : BaseConfiguration<LiveSessionParticipant>
{
    public override void Configure(EntityTypeBuilder<LiveSessionParticipant> builder)
    {
        base.Configure(builder);

        builder.ToTable("LiveSessionParticipants");

        // هر کارجو فقط یک بار می‌تواند در یک جلسه شرکت کند
        builder.HasIndex(participant => new
            {
                participant.LiveSessionId,
                participant.JuniorId
            })
            .IsUnique();

        builder.HasIndex(participant => participant.JuniorId);

        builder.HasOne(participant => participant.LiveSession)
            .WithMany(session => session.Participants)
            .HasForeignKey(participant => participant.LiveSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(participant => participant.Junior)
            .WithMany(user => user.LiveSessionParticipations)
            .HasForeignKey(participant => participant.JuniorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
