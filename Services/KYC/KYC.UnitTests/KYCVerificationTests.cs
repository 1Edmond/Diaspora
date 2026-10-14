using FluentAssertions;
using KYC.Application.Abstractions;
using KYC.Application.Commands;
using KYC.Application.Handlers.Commands;
using KYC.Domain.Entities;
using KYC.Infrastructure.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.Messaging.Events.Kyc;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;
using Xunit;

namespace KYC.UnitTests;

public class KYCVerificationTests
{
    private static KYCVerification CreateTestVerification(Guid userId)
    {
        var address = Address.Create("123 Main St", "Lome", "Maritime", "TG", "00228").Value;
        return KYCVerification.Create(
            userId,
            "John",
            "Doe",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "Togolese",
            address,
            "system").Value;
    }

    [Fact]
    public void Submit_WhenCountryRequirementsSuppliedAndMet_ShouldSucceed()
    {
        var verification = CreateTestVerification(Guid.NewGuid());
        verification.AddDocument("Passport", "pass.pdf", "/files/pass.pdf", 1024);
        verification.AddDocument("Visa", "visa.pdf", "/files/visa.pdf", 1024);

        var countryRequirements = new List<string> { "Passport", "Visa" };
        var result = verification.Submit(countryRequirements);

        result.IsSuccess.Should().BeTrue();
        verification.Status.Should().Be(KYCStatus.Pending);
        verification.SubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public void Submit_WhenCountryRequirementMissing_ShouldFailWithMissingList()
    {
        var verification = CreateTestVerification(Guid.NewGuid());
        verification.AddDocument("Passport", "pass.pdf", "/files/pass.pdf", 1024);

        var countryRequirements = new List<string> { "Passport", "Visa", "ArrivalProof" };
        var result = verification.Submit(countryRequirements);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Visa");
        result.Error.Should().Contain("ArrivalProof");
        verification.Status.Should().Be(KYCStatus.Draft);
    }

    [Fact]
    public void Submit_WhenNoRequirementsSupplied_ShouldFallbackToIdentityAndProofOfAddress()
    {
        var verification = CreateTestVerification(Guid.NewGuid());
        verification.AddDocument("IdentityDocument", "id.pdf", "/files/id.pdf", 1024);
        verification.AddDocument("ProofOfAddress", "proof.pdf", "/files/proof.pdf", 1024);

        var result = verification.Submit();

        result.IsSuccess.Should().BeTrue();
        verification.Status.Should().Be(KYCStatus.Pending);
    }

    [Fact]
    public void CountryKycRequirement_CreateAndDeactivate_ShouldUpdateState()
    {
        var id = Guid.NewGuid();
        var profileTypeId = Guid.NewGuid();
        var docTypeId = Guid.NewGuid();

        var req = CountryKycRequirement.Create(id, profileTypeId, docTypeId, "VISA", "Visa de sejour", true);

        req.Id.Should().Be(id);
        req.ProfileTypeId.Should().Be(profileTypeId);
        req.DocumentTypeId.Should().Be(docTypeId);
        req.DocumentTypeCode.Should().Be("VISA");
        req.Purpose.Should().Be("Visa de sejour");
        req.IsRequired.Should().BeTrue();
        req.IsActive.Should().BeTrue();

        req.Deactivate();
        req.IsActive.Should().BeFalse();

        req.Activate();
        req.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ProfileDirectoryConsumer_Upsert_ShouldHandleLifecycleCorrectly()
    {
        var repo = Substitute.For<IProfileDirectoryRepository>();
        var db = Substitute.For<IKYCDbContext>();
        var logger = Substitute.For<ILogger<ProfileDirectoryConsumer>>();

        var consumer = new ProfileDirectoryConsumer(repo, db, logger);

        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // 1. Profile Created
        await consumer.Handle(new ProfileCreatedIntegrationEvent(profileId, userId, "+22890000000"));
        await repo.Received(1).AddAsync(Arg.Is<ProfileDirectoryEntry>(e => e.Id == profileId && e.UserId == userId), Arg.Any<CancellationToken>());

        // 2. Profile Activated
        var entry = ProfileDirectoryEntry.Create(profileId, userId);
        repo.GetByIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(entry);

        await consumer.Handle(new ProfileActivatedIntegrationEvent(profileId, Guid.NewGuid(), userId, "TG", "XOF", DateTime.UtcNow, "John Doe"));
        entry.IsActivated.Should().BeTrue();
        entry.DisplayName.Should().Be("John Doe");
        entry.CountryCode.Should().Be("TG");

        // 3. Profile Suspended
        await consumer.Handle(new ProfileSuspendedIntegrationEvent(profileId, userId));
        entry.IsSuspended.Should().BeTrue();
    }
}
