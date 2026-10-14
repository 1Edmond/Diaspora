using FluentAssertions;
using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Extensions;
using Marketplace.Application.Handlers.Queries;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class MarketplaceQueryHandlersDtoEnrichmentTests
{
    private static ProfileDirectoryEntry CreateEntry(
        Guid userId,
        Guid profileId,
        string displayName = "Jean Dupont",
        string country = "TG",
        bool isVerified = true)
    {
        var entry = ProfileDirectoryEntry.Create(profileId, userId);
        entry.ApplyActivation(displayName, country, "XOF");
        if (isVerified)
            entry.ApplyVerification();
        return entry;
    }

    [Fact]
    public void MapToDto_WhenProfilesSupplied_ShouldPopulateProfileFields()
    {
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var requesterProfileId = Guid.NewGuid();
        var providerProfileId = Guid.NewGuid();

        var requesterEntry = CreateEntry(requesterId, requesterProfileId, "Requester User", "SN", isVerified: false);
        var providerEntry = CreateEntry(providerId, providerProfileId, "Provider User", "TG", isVerified: true);

        var request = ServiceRequest.Create(
            Guid.NewGuid(), requesterId, providerId, "Need service", 5000m, "XOF");

        var dto = request.MapToDto(requesterEntry, providerEntry);

        dto.RequesterId.Should().Be(requesterId);
        dto.RequesterProfileId.Should().Be(requesterProfileId);
        dto.RequesterName.Should().Be("Requester User");

        dto.ProviderId.Should().Be(providerId);
        dto.ProviderProfileId.Should().Be(providerProfileId);
        dto.ProviderName.Should().Be("Provider User");
        dto.ProviderIsVerified.Should().BeTrue();
        dto.ProviderCountryCode.Should().Be("TG");
    }

    [Fact]
    public void MapToDto_WhenProfilesNull_ShouldFallbackToDefaultsWithoutThrowing()
    {
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var request = ServiceRequest.Create(
            Guid.NewGuid(), requesterId, providerId, "Need service", null, null);

        var dto = request.MapToDto(null, null);

        dto.RequesterProfileId.Should().BeNull();
        dto.RequesterName.Should().BeEmpty();

        dto.ProviderProfileId.Should().BeNull();
        dto.ProviderName.Should().BeEmpty();
        dto.ProviderIsVerified.Should().BeFalse();
        dto.ProviderCountryCode.Should().BeNull();
    }
}
