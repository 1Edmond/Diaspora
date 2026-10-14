using FluentAssertions;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Consumers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Profile;
using Xunit;

namespace Marketplace.UnitTests;

public class ProfileDirectoryConsumerTests
{
    private readonly IProfileDirectoryRepository _repository = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<ProfileDirectoryConsumer> _logger = Substitute.For<ILogger<ProfileDirectoryConsumer>>();
    private readonly ProfileDirectoryConsumer _consumer;

    public ProfileDirectoryConsumerTests()
    {
        _consumer = new ProfileDirectoryConsumer(_repository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ProfileCreated_CreatesNewEntry()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repository.GetByIdAsync(profileId).Returns((ProfileDirectoryEntry?)null);

        var @event = new ProfileCreatedIntegrationEvent(profileId, userId, "+22890000000");

        await _consumer.Handle(@event);

        await _repository.Received(1).AddAsync(Arg.Is<ProfileDirectoryEntry>(e =>
            e.Id == profileId && e.UserId == userId && !e.IsActivated && !e.IsVerified && !e.IsSuspended));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ProfileCreated_WhenAlreadyExists_IsIdempotent()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = ProfileDirectoryEntry.Create(profileId, userId);
        _repository.GetByIdAsync(profileId).Returns(existing);

        var @event = new ProfileCreatedIntegrationEvent(profileId, userId, "+22890000000");

        await _consumer.Handle(@event);

        await _repository.DidNotReceive().AddAsync(Arg.Any<ProfileDirectoryEntry>());
        await _repository.Received(1).UpdateAsync(existing);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_OrderInverse_VerifiedBeforeCreated_CreatesAndPreservesVerification()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // 1. Verified arrives first when entry does not exist yet
        _repository.GetByIdAsync(profileId).Returns((ProfileDirectoryEntry?)null);

        ProfileDirectoryEntry? capturedEntry = null;
        await _repository.AddAsync(Arg.Do<ProfileDirectoryEntry>(e => capturedEntry = e));

        var verifiedEvent = new ProfileVerifiedIntegrationEvent(profileId, userId, "admin-1", DateTime.UtcNow);
        await _consumer.Handle(verifiedEvent);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Id.Should().Be(profileId);
        capturedEntry.UserId.Should().Be(userId);
        capturedEntry.IsVerified.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync();

        // 2. Created arrives after
        _repository.GetByIdAsync(profileId).Returns(capturedEntry);
        var createdEvent = new ProfileCreatedIntegrationEvent(profileId, userId, "+22890000000");
        await _consumer.Handle(createdEvent);

        capturedEntry.IsVerified.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(capturedEntry);
    }

    [Fact]
    public async Task Handle_ConflictUserId_RefusesToOverwriteAndDoesNotSaveChanges()
    {
        var profileId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var conflictingUserId = Guid.NewGuid();

        var existing = ProfileDirectoryEntry.Create(profileId, existingUserId);
        _repository.GetByIdAsync(profileId).Returns(existing);

        var @event = new ProfileActivatedIntegrationEvent(
            profileId, Guid.NewGuid(), conflictingUserId, "TG", "XOF", DateTime.UtcNow, "Hacker");

        await _consumer.Handle(@event);

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<ProfileDirectoryEntry>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
        existing.UserId.Should().Be(existingUserId);
    }

    [Fact]
    public async Task Handle_ProfileActivated_AppliesActivationAndNormalizesFields()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entry = ProfileDirectoryEntry.Create(profileId, userId);
        _repository.GetByIdAsync(profileId).Returns(entry);

        var @event = new ProfileActivatedIntegrationEvent(
            profileId, Guid.NewGuid(), userId, "tg", "xof", DateTime.UtcNow, "  John Doe  ");

        await _consumer.Handle(@event);

        entry.IsActivated.Should().BeTrue();
        entry.IsSuspended.Should().BeFalse();
        entry.DisplayName.Should().Be("John Doe");
        entry.CountryCode.Should().Be("TG");
        entry.Currency.Should().Be("XOF");
        entry.IsEligibleProvider.Should().BeTrue();

        await _repository.Received(1).UpdateAsync(entry);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ProfileSuspended_MarksSuspendedAndNotEligible()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entry = ProfileDirectoryEntry.Create(profileId, userId);
        entry.ApplyActivation("John Doe", "TG", "XOF");
        entry.IsEligibleProvider.Should().BeTrue();

        _repository.GetByIdAsync(profileId).Returns(entry);

        var @event = new ProfileSuspendedIntegrationEvent(profileId, userId);
        await _consumer.Handle(@event);

        entry.IsSuspended.Should().BeTrue();
        entry.IsEligibleProvider.Should().BeFalse();

        await _repository.Received(1).UpdateAsync(entry);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_EmptyIdentifiers_IgnoresEvent()
    {
        var @event = new ProfileCreatedIntegrationEvent(Guid.Empty, Guid.Empty, "+22800000000");
        await _consumer.Handle(@event);

        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_KycArrivalProofApproved_MarksKycApprovedAndVerified()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entry = ProfileDirectoryEntry.Create(profileId, userId);
        _repository.GetByIdAsync(profileId).Returns(entry);

        var @event = new Shared.Messaging.Events.Kyc.KycArrivalProofApprovedIntegrationEvent(
            Guid.NewGuid(), userId, profileId, "TG", DateTime.UtcNow);

        await _consumer.Handle(@event);

        entry.IsKycApproved.Should().BeTrue();
        entry.IsVerified.Should().BeTrue();

        await _repository.Received(1).UpdateAsync(entry);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
