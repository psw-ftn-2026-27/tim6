using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Conversations;

namespace Social.Infrastructure.Persistence.Conversations;

internal static class ConversationConfiguration
{
    public static void ConfigureConversations(this ModelBuilder modelBuilder)
    {
        ConfigureConversation(modelBuilder.Entity<Conversation>());
        ConfigureParticipant(modelBuilder.Entity<ConversationParticipant>());
        ConfigureMessage(modelBuilder.Entity<ChatMessage>());
    }

    private static void ConfigureConversation(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.OwnsOne(conversation => conversation.Context, context =>
        {
            context.Property(value => value.Type).HasColumnName("ContextType").HasMaxLength(50);
            context.Property(value => value.ContextId).HasColumnName("ContextId");
            context.Ignore(value => value.IsDirect);
            context.HasIndex(value => new { value.Type, value.ContextId });
        });
        builder.HasMany(conversation => conversation.Participants)
            .WithOne().HasForeignKey("ConversationId").IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(conversation => conversation.Participants).AutoInclude();
    }

    private static void ConfigureParticipant(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");
        builder.Property(participant => participant.Id).ValueGeneratedNever();
        builder.Property(participant => participant.Role).HasConversion<string>().HasMaxLength(20);
        builder.OwnsOne(participant => participant.Mute, mute => mute.ToJson());
        builder.HasIndex("ConversationId", nameof(ConversationParticipant.UserId)).IsUnique();
    }

    private static void ConfigureMessage(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Ignore(message => message.IsDeleted);
        builder.OwnsOne(message => message.Content, content =>
            content.Property(value => value.Value).HasColumnName("Content"));
        builder.HasOne<Conversation>().WithMany().HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(message => new { message.ConversationId, message.CreatedAt, message.Id });
    }
}
