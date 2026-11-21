using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ConversationParticipantConfiguration : BaseConfiguration<ConversationParticipant>
{
    public override void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        base.Configure(builder);

        builder.ToTable("ConversationParticipants");

        builder.Property(participant => participant.JoinedAt).IsRequired();

        // هر کاربر فقط یک بار در هر مکالمه ثبت می‌شود
        builder.HasIndex(participant => new { participant.ConversationId, participant.UserId }).IsUnique();

        builder.HasOne(participant => participant.Conversation)
            .WithMany(conversation => conversation.Participants)
            .HasForeignKey(participant => participant.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(participant => participant.User)
            .WithMany(user => user.ConversationParticipations)
            .HasForeignKey(participant => participant.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}