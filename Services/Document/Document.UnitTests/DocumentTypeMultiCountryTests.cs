using Document.API.Controllers;
using Document.Application.DTOs;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Shared.Domain.Translations;
using Xunit;

namespace Document.UnitTests;

public class DocumentTypeMultiCountryTests
{
    [Fact]
    public void DocumentType_CreateGlobal_ShouldHaveEmptyGuidProfileTypeId()
    {
        var result = DocumentType.Create(
            Guid.Empty,
            "Passport",
            "PASSPORT",
            "Global identity document",
            true,
            30,
            true);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProfileTypeId.Should().Be(Guid.Empty);
        result.Value.Code.Should().Be("PASSPORT");
    }

    [Fact]
    public void DocumentType_CreateCountrySpecific_ShouldRetainProfileTypeId()
    {
        var countryId = Guid.NewGuid();
        var result = DocumentType.Create(
            countryId,
            "Certificat de nationalite",
            "NATIONALITY_CERT",
            "Togolese certificate of nationality",
            false,
            0,
            false);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProfileTypeId.Should().Be(countryId);
        result.Value.Code.Should().Be("NATIONALITY_CERT");
    }

    [Fact]
    public async Task GetAll_WhenProfileTypeIdProvided_ShouldFilterAndResolveTranslations()
    {
        var repo = Substitute.For<IDocumentTypeRepository>();
        var transRepo = Substitute.For<ITranslationRepository<DocumentType>>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<DocumentTypeController>>();

        currentUser.DataLanguage.Returns("en");

        var countryId = Guid.NewGuid();
        var globalDoc = DocumentType.Create(Guid.Empty, "Passeport", "PASSPORT", "Passeport international", true, 30, true).Value;
        var countryDoc = DocumentType.Create(countryId, "Carte Consulaire", "CONSULAR_CARD", "Carte d'immatriculation", true, 60, false).Value;

        var docList = new List<DocumentType> { globalDoc, countryDoc };
        repo.GetActiveByProfileTypeAsync(countryId, Arg.Any<CancellationToken>()).Returns(docList);

        IReadOnlyDictionary<Guid, IReadOnlyCollection<Translation<DocumentType>>> translations =
            new Dictionary<Guid, IReadOnlyCollection<Translation<DocumentType>>>
            {
                [globalDoc.Id] = new List<Translation<DocumentType>>
                {
                    Translation<DocumentType>.Create(globalDoc.Id, "en", new Dictionary<string, string>
                    {
                        ["Name"] = "Passport",
                        ["Description"] = "International Passport"
                    })
                },
                [countryDoc.Id] = new List<Translation<DocumentType>>
                {
                    Translation<DocumentType>.Create(countryDoc.Id, "en", new Dictionary<string, string>
                    {
                        ["Name"] = "Consular Card",
                        ["Description"] = "Consular registration card"
                    })
                }
            };

        transRepo.GetByOwnerIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(translations);

        var controller = new DocumentTypeController(repo, transRepo, currentUser, unitOfWork, logger);
        var actionResult = await controller.GetAll(countryId, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<DocumentTypeDto>>().Subject.ToList();

        dtos.Should().HaveCount(2);

        var globalDto = dtos.First(d => d.Code == "PASSPORT");
        globalDto.Name.Should().Be("Passport");
        globalDto.Description.Should().Be("International Passport");
        globalDto.ProfileTypeId.Should().Be(Guid.Empty);

        var countryDto = dtos.First(d => d.Code == "CONSULAR_CARD");
        countryDto.Name.Should().Be("Consular Card");
        countryDto.Description.Should().Be("Consular registration card");
        countryDto.ProfileTypeId.Should().Be(countryId);

        await repo.Received(1).GetActiveByProfileTypeAsync(countryId, Arg.Any<CancellationToken>());
    }
}
