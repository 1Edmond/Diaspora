using Document.Application.Interfaces;
using Document.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Profile;
using System;
using System.Threading.Tasks;

namespace Document.Infrastructure.Consumers;

/// <summary>
/// Alimente la projection locale des profils à partir des événements du Profile Service.
/// Idempotent et tolérant aux réceptions désordonnées.
/// </summary>
public class ProfileDirectoryConsumer(
    IProfileDirectoryRepository _repository,
    IUnitOfWork _unitOfWork,
    ILogger<ProfileDirectoryConsumer> _logger)
{
    public Task Handle(ProfileCreatedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, _ => { });

    public Task Handle(ProfileActivatedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId,
            entry => entry.ApplyActivation(message.DisplayName, message.CountryCode, message.Currency));

    public Task Handle(ProfileVerifiedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry => entry.ApplyVerification());

    public Task Handle(ProfileSuspendedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry => entry.ApplySuspension());

    private async Task UpsertAsync(Guid profileId, Guid userId, Action<ProfileDirectoryEntry> apply)
    {
        if (profileId == Guid.Empty || userId == Guid.Empty)
        {
            _logger.LogWarning(
                "Ignoring profile event with empty identifier (ProfileId={ProfileId}, UserId={UserId})",
                profileId, userId);
            return;
        }

        var entry = await _repository.GetByIdAsync(profileId);

        if (entry is null)
        {
            entry = ProfileDirectoryEntry.Create(profileId, userId);
            apply(entry);
            await _repository.AddAsync(entry);
        }
        else
        {
            if (entry.UserId != userId)
            {
                _logger.LogError(
                    "Profile {ProfileId} is already bound to user {ExistingUserId}, ignoring event for user {UserId}",
                    profileId, entry.UserId, userId);
                return;
            }

            apply(entry);
            await _repository.UpdateAsync(entry);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
