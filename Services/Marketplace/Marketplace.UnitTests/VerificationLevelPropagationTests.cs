using FluentAssertions;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Consumers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Profile;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class VerificationLevelPropagationTests
{
    private readonly IProfileDirectoryRepository _repository = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<ProfileDirectoryConsumer> _logger = Substitute.For<ILogger<ProfileDirectoryConsumer>>();
    private readonly ProfileDirectoryConsumer _consumer;

    public VerificationLevelPropagationTests()
    {
        _consumer = new ProfileDirectoryConsumer(_repository, _unitOfWork, _logger);
    }

    [Fact]
    public void ProfileDirectoryEntry_WhenCreated_ShouldHaveL0RegisteredLevel()
    {
        var entry = ProfileDirectoryEntry.Create(Guid.NewGuid(), Guid.NewGuid());
        entry.Level.Should().Be(VerificationLevel.L0_Registered);
    }

    [Fact]
    public void ProfileDirectoryEntry_WhenVerifiedOrKycApproved_ShouldElevateToL2()
    {
        var entry = ProfileDirectoryEntry.Create(Guid.NewGuid(), Guid.NewGuid());
        entry.ApplyVerification();
        entry.Level.Should().Be(VerificationLevel.L2_IdentityVerified);

        var entryKyc = ProfileDirectoryEntry.Create(Guid.NewGuid(), Guid.NewGuid());
        entryKyc.ApplyKycApproval();
        entryKyc.Level.Should().Be(VerificationLevel.L2_IdentityVerified);
    }

    [Fact]
    public void ProfileDirectoryEntry_WhenSetVerificationLevelExplicitly_ShouldUpdate()
    {
        var entry = ProfileDirectoryEntry.Create(Guid.NewGuid(), Guid.NewGuid());
        entry.SetVerificationLevel(VerificationLevel.L3_EnhancedVerified);
        entry.Level.Should().Be(VerificationLevel.L3_EnhancedVerified);
    }

    [Fact]
    public async Task Consumer_WhenProfileCreatedEventReceivedWithL1_ShouldStoreL1()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repository.GetByIdAsync(profileId).Returns((ProfileDirectoryEntry?)null);

        var evt = new ProfileCreatedIntegrationEvent(
            profileId,
            userId,
            "+22890000000",
            VerificationLevel.L1_IdentityDeclared);

        await _consumer.Handle(evt);

        await _repository.Received(1).AddAsync(Arg.Is<ProfileDirectoryEntry>(e =>
            e.Id == profileId &&
            e.UserId == userId &&
            e.Level == VerificationLevel.L1_IdentityDeclared));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
