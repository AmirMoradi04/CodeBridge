using CodeBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBridge.Infrastructure.Configurations;

public class ConversationConfiguration : BaseConfiguration<Conversation>
{
    public override void Configure(EntityTypeBuilder<Conversation> builder)
    {
        base.Configure(builder);

        builder.ToTable("Conversations");

        builder.Property(conversation => conversation.Title).IsRequired().HasMaxLength(200);

        builder.Property(conversation => conversation.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(conversation => conversation.Type);

        // فقط گروه‌های پروژه به پروژه متصل‌اند
        builder.HasOne(conversation => conversation.Project)
            .WithMany()
            .HasForeignKey(conversation => conversation.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(conversation => conversation.Participants)
            .WithOne(participant => participant.Conversation)
            .HasForeignKey(participant => participant.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(conversation => conversation.Messages)
            .WithOne(message => message.Conversation)
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}