using FluentAssertions;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Profile.Domain.Events;
using Shared.Domain.ValueObjects;
using Xunit;

namespace Profile.UnitTests;

public class ProfileTests
{
    private static Domain.Entities.Profile CreateTestProfile()
    {
        var phone = PhoneNumber.Create("90000000", "+228").Value;
        var profile = Domain.Entities.Profile.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            phone,
            "Koffi",
            "Mensah",
            new DateOnly(1990, 1, 1)).Value;

        return profile;
    }

    [Fact]
    public void Activate_WhenStatusIsPendingVerification_ShouldFailWithInformativeError()
    {
        var profile = CreateTestProfile();
        profile.Status.Should().Be(ProfileStatus.PendingVerification);

        var result = profile.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("doit d'abord être vérifié");
    }

    [Fact]
    public void Activate_WhenVerifiedAndKycL2Approved_ShouldSucceedAndEmitDomainEvent()
    {
        var profile = CreateTestProfile();
        profile.Verify("admin-1");
        profile.ApplyKycApproval(Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified);

        profile.Status.Should().Be(ProfileStatus.Activate);
        profile.DomainEvents.Should().ContainSingle(e => e is ProfileActivatedDomainEvent);
    }

    [Fact]
    public void Activate_WhenStatusIsSuspended_ShouldFailWithCorrectErrorMessage()
    {
        var profile = CreateTestProfile();
        profile.Suspend();
        profile.Status.Should().Be(ProfileStatus.Suspended);

        var result = profile.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Ce profil est suspendu et ne peut pas être activé.");
        profile.Status.Should().Be(ProfileStatus.Suspended);
    }

    [Fact]
    public void Activate_WhenStatusIsAlreadyActivate_ShouldFail()
    {
        var profile = CreateTestProfile();
        profile.Verify("admin-1");
        profile.ApplyKycApproval(Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified);
        profile.Status.Should().Be(ProfileStatus.Activate);

        var result = profile.Activate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Ce profil est déjà actif.");
    }

    [Fact]
    public void Suspend_WhenActiveOrPending_ShouldSucceed()
    {
        var profile = CreateTestProfile();

        var result = profile.Suspend();

        result.IsSuccess.Should().BeTrue();
        profile.Status.Should().Be(ProfileStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldFail()
    {
        var profile = CreateTestProfile();
        profile.Suspend();

        var result = profile.Suspend();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already suspended");
    }
}
