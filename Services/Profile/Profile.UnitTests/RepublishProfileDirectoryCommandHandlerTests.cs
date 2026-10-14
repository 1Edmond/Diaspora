using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Profile.Application.Commands;
using Profile.Application.Handlers.Commands;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.Messaging.Events.Profile;
using Wolverine;
using Xunit;

namespace Profile.UnitTests;

public class RepublishProfileDirectoryCommandHandlerTests
{
    private readonly IProfileRepository _repository = Substitute.For<IProfileRepository>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly ILogger<RepublishProfileDirectoryCommandHandler> _logger =
        Substitute.For<ILogger<RepublishProfileDirectoryCommandHandler>>();
    private readonly RepublishProfileDirectoryCommandHandler _handler;

    public RepublishProfileDirectoryCommandHandlerTests()
    {
        _handler = new RepublishProfileDirectoryCommandHandler(_repository, _messageBus, _logger);
    }

    private static Domain.Entities.Profile CreateProfile(ProfileStatus status, bool verified = false)
    {
        var phone = PhoneNumber.Create("90112233", "+228").Value;
        var profile = Domain.Entities.Profile.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            phone,
            "Afi",
            "Abalo",
            new DateOnly(1995, 5, 10)).Value;

        if (verified)
        {
            profile.Verify("admin-1");
        }

        if (status == ProfileStatus.Activate)
        {
            profile.Activate();
        }
        else if (status == ProfileStatus.Suspended)
        {
            profile.Suspend();
        }

        return profile;
    }

    [Fact]
    public async Task Handle_WhenNoProfiles_ReturnsZeroProcessedAndPublished()
    {
        _repository.GetTotalCountAsync(Arg.Any<CancellationToken>()).Returns(0);

        var result = await _handler.Handle(new RepublishProfileDirectoryCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalProfilesProcessed.Should().Be(0);
        result.Value.TotalEventsPublished.Should().Be(0);
        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithDifferentProfiles_PublishesExpectedIntegrationEvents()
    {
        // Arrange: 3 profiles:
        // 1. PendingVerification (only Created)
        // 2. Verified (Created + Verified)
        // 3. Activated with Verified trace (Created + Verified + Activated)
        // 4. Suspended (Created + Suspended)
        var pendingProfile = CreateProfile(ProfileStatus.PendingVerification);
        var verifiedProfile = CreateProfile(ProfileStatus.Verified, verified: true);
        var activeProfile = CreateProfile(ProfileStatus.Activate, verified: true);
        var suspendedProfile = CreateProfile(ProfileStatus.Suspended);

        var list = new List<Domain.Entities.Profile> { pendingProfile, verifiedProfile, activeProfile, suspendedProfile };

        _repository.GetTotalCountAsync(Arg.Any<CancellationToken>()).Returns(list.Count);
        _repository.GetBatchWithProfileTypeAsync(0, 500, Arg.Any<CancellationToken>()).Returns(list);

        // Act
        var result = await _handler.Handle(new RepublishProfileDirectoryCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalProfilesProcessed.Should().Be(4);
        // Events:
        // pendingProfile: Created (1)
        // verifiedProfile: Created + Verified (2)
        // activeProfile: Created + Verified + Activated (3)
        // suspendedProfile: Created + Suspended (2)
        // Total = 8
        result.Value.TotalEventsPublished.Should().Be(8);

        await _messageBus.Received(4).PublishAsync(Arg.Any<ProfileCreatedIntegrationEvent>());
        await _messageBus.Received(2).PublishAsync(Arg.Any<ProfileVerifiedIntegrationEvent>());
        await _messageBus.Received(1).PublishAsync(Arg.Any<ProfileActivatedIntegrationEvent>());
        await _messageBus.Received(1).PublishAsync(Arg.Any<ProfileSuspendedIntegrationEvent>());
    }

    [Fact]
    public async Task Handle_WhenPublishThrowsForOneProfile_ContinuesProcessingRemaining()
    {
        var profile1 = CreateProfile(ProfileStatus.PendingVerification);
        var profile2 = CreateProfile(ProfileStatus.PendingVerification);
        var list = new List<Domain.Entities.Profile> { profile1, profile2 };

        _repository.GetTotalCountAsync(Arg.Any<CancellationToken>()).Returns(2);
        _repository.GetBatchWithProfileTypeAsync(0, 500, Arg.Any<CancellationToken>()).Returns(list);

        // Throws for profile1
        _messageBus.PublishAsync(Arg.Is<ProfileCreatedIntegrationEvent>(e => e.ProfileId == profile1.Id))
            .Returns<ValueTask>(_ => throw new InvalidOperationException("RabbitMQ connection dropped"));

        // Act
        var result = await _handler.Handle(new RepublishProfileDirectoryCommand(), CancellationToken.None);

        // Assert: profile2 was processed despite profile1 failing
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalProfilesProcessed.Should().Be(1);
        result.Value.TotalEventsPublished.Should().Be(1);
        await _messageBus.Received(1).PublishAsync(Arg.Is<ProfileCreatedIntegrationEvent>(e => e.ProfileId == profile2.Id));
    }
}
