using Document.Application.Handlers.Queries;
using Document.Application.Interfaces;
using Document.Application.Queries;
using Document.Domain.Entities;
using Document.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Document.UnitTests;

public class GetPendingDocumentsQueryTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly GetPendingDocumentsQueryHandler _handler;

    public GetPendingDocumentsQueryTests()
    {
        _handler = new GetPendingDocumentsQueryHandler(_documentRepository);
    }

    private static DocumentType CreateActiveDocumentType(Guid id)
    {
        var type = DocumentType.Create(Guid.NewGuid(), "Passeport", "PASSPORT", "Description", true, 30, false).Value;
        typeof(DocumentType).GetProperty(nameof(DocumentType.Id))!.SetValue(type, id);
        return type;
    }

    private static Domain.Entities.Document CreateDocument(Guid id, Guid profileId, DocumentType type)
    {
        var doc = Domain.Entities.Document.Create(
            profileId,
            type,
            "path/to/encrypted/file.dat",
            "passport.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow.AddYears(1),
            DateTime.UtcNow,
            "Direction Générale de la Documentation Nationale").Value;

        typeof(Domain.Entities.Document).GetProperty(nameof(Domain.Entities.Document.Id))!.SetValue(doc, id);
        return doc;
    }

    [Fact]
    public async Task Handle_WhenPendingDocumentsExist_ShouldReturnPagedPendingDocuments()
    {
        // Arrange
        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc1 = CreateDocument(Guid.NewGuid(), Guid.NewGuid(), docType);
        var doc2 = CreateDocument(Guid.NewGuid(), Guid.NewGuid(), docType);
        var pendingDocs = new List<Domain.Entities.Document> { doc1, doc2 };

        _documentRepository.GetByStatusAsync(DocumentStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(pendingDocs);

        var query = new GetPendingDocumentsQuery(PageNumber: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Select(d => d.Id).Should().Contain([doc1.Id, doc2.Id]);
        result.Value.Items.All(d => d.Status == DocumentStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoPendingDocuments_ShouldReturnEmptyPagedList()
    {
        // Arrange
        _documentRepository.GetByStatusAsync(DocumentStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Document>());

        var query = new GetPendingDocumentsQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldPaginateCorrectly()
    {
        // Arrange
        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc1 = CreateDocument(Guid.NewGuid(), Guid.NewGuid(), docType);
        var doc2 = CreateDocument(Guid.NewGuid(), Guid.NewGuid(), docType);
        var pendingDocs = new List<Domain.Entities.Document> { doc1, doc2 };

        _documentRepository.GetByStatusAsync(DocumentStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(pendingDocs);

        var query = new GetPendingDocumentsQuery(PageNumber: 2, PageSize: 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Id.Should().Be(doc2.Id);
    }
}
