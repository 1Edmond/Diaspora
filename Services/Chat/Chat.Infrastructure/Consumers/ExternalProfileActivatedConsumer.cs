using Chat.Application.Commands;
using Chat.Application.Handlers.Commands;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Consumers;

public sealed class ExternalProfileActivatedConsumer(
    CreateChatProfileCommandHandler createHandler,
    IConversationRepository conversationRepository,
    IChatProfileRepository chatProfileRepository,
    Shared.Domain.Interfaces.IUnitOfWork unitOfWork,
    IIdempotencyService idempotency,
    ILogger<ExternalProfileActivatedConsumer> logger)
{
    public async Task Handle(ProfileActivatedIntegrationEvent @event)
    {
        await idempotency.ProcessOnceAsync(
            @event.EventId,
            nameof(ProfileActivatedIntegrationEvent),
            async () =>
            {

                var existChatProfile = await chatProfileRepository.ExistsByUserIdAsync(@event.UserId);

                if (existChatProfile)
                    return Result.Failure("Chat Profile already exists");


                // 1. Créer le ChatProfile
                var command = new CreateChatProfileCommand(
                    @event.UserId,
                    @event.ProfileId,
                    @event.ProfileTypeId,
                    @event.DisplayName ?? @event.UserId.ToString());

                var result = await createHandler.Handle(command, CancellationToken.None);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Failed to create ChatProfile for user {UserId}: {Error}",
                        @event.UserId, result.Error);
                    return result;
                }

                // 2. Auto-join toutes les Communities actives
                var communities = await conversationRepository
                    .GetCommunitiesAsync(CancellationToken.None);

                var chatProfile = await chatProfileRepository
                    .GetByUserIdAsync(@event.UserId, CancellationToken.None);

                if (chatProfile is not null)
                {
                    foreach (var community in communities)
                    {
                        var conv = await conversationRepository
                            .GetWithParticipantsAsync(community.Id, CancellationToken.None);
                        conv?.AddParticipant(chatProfile.Id);
                        if (conv is not null)
                            await conversationRepository.UpdateAsync(conv, CancellationToken.None);
                    }
                    await unitOfWork.SaveChangesAsync(CancellationToken.None);
                }

                logger.LogInformation(
                    "ChatProfile created and joined communities for user {UserId}", @event.UserId);
                return result;
            });
    }
}
