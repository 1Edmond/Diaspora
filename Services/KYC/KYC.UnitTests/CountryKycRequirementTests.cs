using FluentAssertions;
using KYC.Application.Commands;
using KYC.Application.Dtos;
using KYC.Application.Handlers.Commands;
using KYC.Application.Handlers.Queries;
using KYC.Application.Interfaces;
using KYC.Application.Queries;
using KYC.Domain.Entities;
using NSubstitute;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KYC.UnitTests;

public class CountryKycRequirementTests
{
    private readonly ICountryKycRequirementRepository _repository = Substitute.For<ICountryKycRequirementRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateRequirement_WhenValid_ShouldPersistAndReturnDto()
    {
        var handler = new CreateCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var profileTypeId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        _repository.FirstOrDefaultAsync(Arg.Any<Expression<Func<CountryKycRequirement, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CountryKycRequirement?)null);

        var command = new CreateCountryKycRequirementCommand(
            profileTypeId,
            documentTypeId,
            "CONSULAR_CARD",
            "Proof of consular registration",
            true
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ProfileTypeId.Should().Be(profileTypeId);
        result.Value.DocumentTypeId.Should().Be(documentTypeId);
        result.Value.DocumentTypeCode.Should().Be("CONSULAR_CARD");
        result.Value.Purpose.Should().Be("Proof of consular registration");
        result.Value.IsRequired.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Is<CountryKycRequirement>(r =>
            r.ProfileTypeId == profileTypeId &&
            r.DocumentTypeId == documentTypeId &&
            r.DocumentTypeCode == "CONSULAR_CARD" &&
            r.Purpose == "Proof of consular registration"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRequirement_WhenDuplicateDocumentTypeCode_ShouldFail()
    {
        var handler = new CreateCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var profileTypeId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var existing = CountryKycRequirement.Create(Guid.NewGuid(), profileTypeId, documentTypeId, "PASSPORT", "Existing");

        _repository.FirstOrDefaultAsync(Arg.Any<Expression<Func<CountryKycRequirement, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new CreateCountryKycRequirementCommand(
            profileTypeId,
            documentTypeId,
            "PASSPORT",
            "Passport verification",
            true
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        await _repository.DidNotReceive().AddAsync(Arg.Any<CountryKycRequirement>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000001", "CODE", "Purpose", "ProfileTypeId is required.")]
    [InlineData("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000000", "CODE", "Purpose", "DocumentTypeId is required.")]
    [InlineData("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "", "Purpose", "DocumentTypeCode is required.")]
    [InlineData("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "CODE", "", "Purpose is required.")]
    public async Task CreateRequirement_WhenInvalidInput_ShouldFailValidation(
        string profileTypeGuid, string docTypeGuid, string code, string purpose, string expectedError)
    {
        var handler = new CreateCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var command = new CreateCountryKycRequirementCommand(
            Guid.Parse(profileTypeGuid),
            Guid.Parse(docTypeGuid),
            code,
            purpose,
            true
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task UpdateRequirement_WhenRequirementExists_ShouldUpdateAndPersist()
    {
        var handler = new UpdateCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var id = Guid.NewGuid();
        var requirement = CountryKycRequirement.Create(id, Guid.NewGuid(), Guid.NewGuid(), "VISA", "Old purpose", true);

        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(requirement);

        var command = new UpdateCountryKycRequirementCommand(id, "Updated purpose", false, false);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Purpose.Should().Be("Updated purpose");
        result.Value.IsRequired.Should().BeFalse();
        result.Value.IsActive.Should().BeFalse();

        requirement.Purpose.Should().Be("Updated purpose");
        requirement.IsRequired.Should().BeFalse();
        requirement.IsActive.Should().BeFalse();

        await _repository.Received(1).UpdateAsync(requirement, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRequirement_WhenNotFound_ShouldFail()
    {
        var handler = new UpdateCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((CountryKycRequirement?)null);

        var command = new UpdateCountryKycRequirementCommand(id, "Updated purpose", true, true);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteRequirement_WhenExists_ShouldDeleteAndSave()
    {
        var handler = new DeleteCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var id = Guid.NewGuid();
        var requirement = CountryKycRequirement.Create(id, Guid.NewGuid(), Guid.NewGuid(), "VISA", "Purpose", true);

        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(requirement);

        var command = new DeleteCountryKycRequirementCommand(id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(requirement, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRequirement_WhenNotFound_ShouldFail()
    {
        var handler = new DeleteCountryKycRequirementCommandHandler(_repository, _unitOfWork);
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((CountryKycRequirement?)null);

        var command = new DeleteCountryKycRequirementCommand(id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetRequirements_WithProfileTypeId_ShouldFilterByCountry()
    {
        var handler = new GetCountryKycRequirementsQueryHandler(_repository);
        var profileTypeId = Guid.NewGuid();
        var requirements = new List<CountryKycRequirement>
        {
            CountryKycRequirement.Create(Guid.NewGuid(), profileTypeId, Guid.NewGuid(), "PASSPORT", "P1", true),
            CountryKycRequirement.Create(Guid.NewGuid(), profileTypeId, Guid.NewGuid(), "VISA", "P2", false),
        };

        _repository.GetByProfileTypeIdAsync(profileTypeId, Arg.Any<CancellationToken>())
            .Returns(requirements);

        var query = new GetCountryKycRequirementsQuery(profileTypeId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(r => r.DocumentTypeCode).Should().Contain(new[] { "PASSPORT", "VISA" });
    }

    [Fact]
    public async Task GetRequirements_WithoutProfileTypeId_ShouldReturnAll()
    {
        var handler = new GetCountryKycRequirementsQueryHandler(_repository);
        var requirements = new List<CountryKycRequirement>
        {
            CountryKycRequirement.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "PASSPORT", "P1", true),
            CountryKycRequirement.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VISA", "P2", false),
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(requirements);

        var query = new GetCountryKycRequirementsQuery(null);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public void CountryKycRequirement_Update_ShouldModifyFields()
    {
        var requirement = CountryKycRequirement.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VISA",
            "Initial purpose",
            true);

        requirement.Update("New purpose", false, false);

        requirement.Purpose.Should().Be("New purpose");
        requirement.IsRequired.Should().BeFalse();
        requirement.IsActive.Should().BeFalse();
        requirement.UpdatedAt.Should().NotBeNull();
    }
}
