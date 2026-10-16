using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Profile;
using System;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Consumers;

/// <summary>
/// Alimente la projection locale des profils à partir des événements du Profile Service.
/// Chaque handler est un upsert idempotent : rejouer ou recevoir les événements dans le désordre
/// (ex. Verified avant Created) converge vers le même état final.
/// </summary>
public class ProfileDirectoryConsumer(
    IProfileDirectoryRepository _repository,
    IUnitOfWork _unitOfWork,
    ILogger<ProfileDirectoryConsumer> _logger)
{
    public Task Handle(ProfileCreatedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry =>
        {
            if (message.Level > entry.Level)
                entry.SetVerificationLevel(message.Level);
        });

    public Task Handle(ProfileActivatedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry =>
        {
            entry.ApplyActivation(message.DisplayName, message.CountryCode, message.Currency);
            if (message.Level > entry.Level)
                entry.SetVerificationLevel(message.Level);
        });

    public Task Handle(ProfileVerifiedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry =>
        {
            entry.ApplyVerification(message.VerifiedAt, message.Level);
        });

    public Task Handle(ProfileSuspendedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry => entry.ApplySuspension());

    public Task Handle(Shared.Messaging.Events.Kyc.KycArrivalProofApprovedIntegrationEvent message)
        => UpsertAsync(message.ProfileId, message.UserId, entry => entry.ApplyKycApproval(message.ApprovedLevel));

    public async Task Handle(Shared.Messaging.Events.Kyc.KYCVerificationApprovedIntegrationEvent message)
    {
        var entry = await _repository.GetByUserIdAsync(message.UserId);
        if (entry is not null)
        {
            entry.ApplyKycApproval();
            await _repository.UpdateAsync(entry);
            await _unitOfWork.SaveChangesAsync();
        }
    }

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
                // Incohérence de données source : on refuse d'écraser la correspondance existante.
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
