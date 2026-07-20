using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Persistence;

public sealed class ChatDbContext(
    DbContextOptions<ChatDbContext> options,
    IIntegrationEventMapper integrationEventMapper,
    IDateTimeProvider dateTimeProvider,
    ILogger<ChatDbContext> logger,
    ICurrentUserService currentUserService)
    : BaseDbContext(options, integrationEventMapper, dateTimeProvider, logger, currentUserService)
{
    public DbSet<ChatProfile> ChatProfiles => Set<ChatProfile>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<MentionGroup> MentionGroups => Set<MentionGroup>();
    public DbSet<MentionGroupMember> MentionGroupMembers => Set<MentionGroupMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<MessageMention> MessageMentions => Set<MessageMention>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<CallParticipant> CallParticipants => Set<CallParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
        modelBuilder.HasDefaultSchema("chat");
    }
}
