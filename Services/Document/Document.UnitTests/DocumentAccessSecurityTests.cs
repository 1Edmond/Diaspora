using Document.Application.Commands;
using Document.Application.DTOs;
using Document.Application.Handlers.Commands;
using Document.Application.Handlers.Queries;
using Document.Application.Interfaces;
using Document.Application.Queries;
using Document.Application.Responses;
using Document.Application.Services;
using Document.Domain.Entities;
using Document.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Xunit;

namespace Document.UnitTests;

public class DocumentAccessSecurityTests
{
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IDocumentTypeRepository _documentTypeRepository = Substitute.For<IDocumentTypeRepository>();
    private readonly IFileStorageService _fileStorage = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly DocumentAccessService _accessService;

    public DocumentAccessSecurityTests()
    {
        _accessService = new DocumentAccessService(_profileDirectory, _currentUser);
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

    #region AccessService Tests

    [Fact]
    public async Task DocumentAccessService_Owner_CanAccessDocumentAndProfile()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var entry = ProfileDirectoryEntry.Create(profileId, userId);

        _currentUser.UserId.Returns(userId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(userId).Returns(entry);

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(Guid.NewGuid(), profileId, docType);

        var canAccessProfile = await _accessService.CanAccessProfileAsync(profileId);
        var canAccessDocument = await _accessService.CanAccessDocumentAsync(doc);

        canAccessProfile.Should().BeTrue();
        canAccessDocument.Should().BeTrue();
    }

    [Fact]
    public async Task DocumentAccessService_ThirdParty_CannotAccessDocumentNorProfile()
    {
        var callerUserId = Guid.NewGuid();
        var callerProfileId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();

        var callerEntry = ProfileDirectoryEntry.Create(callerProfileId, callerUserId);

        _currentUser.UserId.Returns(callerUserId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(callerUserId).Returns(callerEntry);

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(Guid.NewGuid(), targetProfileId, docType);

        var canAccessProfile = await _accessService.CanAccessProfileAsync(targetProfileId);
        var canAccessDocument = await _accessService.CanAccessDocumentAsync(doc);

        canAccessProfile.Should().BeFalse();
        canAccessDocument.Should().BeFalse();
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Administrator")]
    [InlineData("Officer")]
    public async Task DocumentAccessService_PrivilegedRole_CanAccessAnyDocumentAndProfile(string role)
    {
        var adminUserId = Guid.NewGuid();
        var anyProfileId = Guid.NewGuid();

        _currentUser.UserId.Returns(adminUserId.ToString());
        _currentUser.IsInRole(role).Returns(true);

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(Guid.NewGuid(), anyProfileId, docType);

        var canAccessProfile = await _accessService.CanAccessProfileAsync(anyProfileId);
        var canAccessDocument = await _accessService.CanAccessDocumentAsync(doc);

        canAccessProfile.Should().BeTrue();
        canAccessDocument.Should().BeTrue();
    }

    #endregion

    #region Upload Handler Security Tests

    [Fact]
    public async Task UploadDocumentCommandHandler_WhenCallerIsOwner_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var entry = ProfileDirectoryEntry.Create(profileId, userId);

        _currentUser.UserId.Returns(userId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(userId).Returns(entry);

        var docTypeId = Guid.NewGuid();
        var docType = CreateActiveDocumentType(docTypeId);
        _documentTypeRepository.GetByIdAsync(docTypeId, Arg.Any<CancellationToken>()).Returns(docType);

        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("doc.pdf");
        file.ContentType.Returns("application/pdf");
        file.Length.Returns(1024);
        file.OpenReadStream().Returns(new MemoryStream(new byte[10]));

        _fileStorage.UploadEncryptedFileAsync(
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success("encrypted/path.dat"));

        var handler = new UploadDocumentCommandHandler(
            _documentRepository, _documentTypeRepository, _fileStorage, _accessService, _unitOfWork);

        var command = new UploadDocumentCommand(profileId, docTypeId, file, DateTime.UtcNow.AddYears(1), null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _documentRepository.Received(1).AddAsync(Arg.Any<Domain.Entities.Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadDocumentCommandHandler_WhenCallerIsThirdParty_ShouldRejectWithAccessDenied()
    {
        var callerUserId = Guid.NewGuid();
        var callerProfileId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();
        var callerEntry = ProfileDirectoryEntry.Create(callerProfileId, callerUserId);

        _currentUser.UserId.Returns(callerUserId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(callerUserId).Returns(callerEntry);

        var file = Substitute.For<IFormFile>();
        var handler = new UploadDocumentCommandHandler(
            _documentRepository, _documentTypeRepository, _fileStorage, _accessService, _unitOfWork);

        var command = new UploadDocumentCommand(targetProfileId, Guid.NewGuid(), file, null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Access denied");
        await _fileStorage.DidNotReceive().UploadEncryptedFileAsync(
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetById Query Handler Security Tests

    [Fact]
    public async Task GetDocumentByIdQueryHandler_WhenThirdParty_ReturnsDocumentNotFoundToHideExistence()
    {
        var callerUserId = Guid.NewGuid();
        var callerProfileId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _currentUser.UserId.Returns(callerUserId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(callerUserId).Returns(ProfileDirectoryEntry.Create(callerProfileId, callerUserId));

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(docId, targetProfileId, docType);
        _documentRepository.GetByIdAsync(docId, Arg.Any<CancellationToken>()).Returns(doc);

        var handler = new GetDocumentByIdQueryHandler(_documentRepository, _accessService);
        var result = await handler.Handle(new GetDocumentByIdQuery(docId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Document not found.");
    }

    [Fact]
    public async Task GetDocumentByIdQueryHandler_WhenOwner_ReturnsDocument()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _currentUser.UserId.Returns(userId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(userId).Returns(ProfileDirectoryEntry.Create(profileId, userId));

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(docId, profileId, docType);
        _documentRepository.GetByIdAsync(docId, Arg.Any<CancellationToken>()).Returns(doc);

        var handler = new GetDocumentByIdQueryHandler(_documentRepository, _accessService);
        var result = await handler.Handle(new GetDocumentByIdQuery(docId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(docId);
    }

    [Fact]
    public async Task GetDocumentByIdQueryHandler_WhenAdmin_ReturnsAnyDocument()
    {
        var adminUserId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _currentUser.UserId.Returns(adminUserId.ToString());
        _currentUser.IsInRole("Admin").Returns(true);

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(docId, targetProfileId, docType);
        _documentRepository.GetByIdAsync(docId, Arg.Any<CancellationToken>()).Returns(doc);

        var handler = new GetDocumentByIdQueryHandler(_documentRepository, _accessService);
        var result = await handler.Handle(new GetDocumentByIdQuery(docId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(docId);
    }

    #endregion

    #region GetByProfile Query Handler Security Tests

    [Fact]
    public async Task GetDocumentsByProfileQueryHandler_WhenThirdParty_ReturnsProfileNotFound()
    {
        var callerUserId = Guid.NewGuid();
        var callerProfileId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();

        _currentUser.UserId.Returns(callerUserId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(callerUserId).Returns(ProfileDirectoryEntry.Create(callerProfileId, callerUserId));

        var handler = new GetDocumentsByProfileQueryHandler(_documentRepository, _accessService);
        var result = await handler.Handle(new GetDocumentsByProfileQuery(targetProfileId, 1, 20), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Profile not found.");
        await _documentRepository.DidNotReceive().GetByExternalProfileIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDocumentsByProfileQueryHandler_WhenOwner_ReturnsDocuments()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUser.UserId.Returns(userId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(userId).Returns(ProfileDirectoryEntry.Create(profileId, userId));

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(Guid.NewGuid(), profileId, docType);
        _documentRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Document> { doc });

        var handler = new GetDocumentsByProfileQueryHandler(_documentRepository, _accessService);
        var result = await handler.Handle(new GetDocumentsByProfileQuery(profileId, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    #endregion

    #region Delete Handler Security Tests

    [Fact]
    public async Task DeleteDocumentCommandHandler_WhenThirdParty_ReturnsDocumentNotFound()
    {
        var callerUserId = Guid.NewGuid();
        var callerProfileId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _currentUser.UserId.Returns(callerUserId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(callerUserId).Returns(ProfileDirectoryEntry.Create(callerProfileId, callerUserId));

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(docId, targetProfileId, docType);
        _documentRepository.GetByIdAsync(docId, Arg.Any<CancellationToken>()).Returns(doc);

        var handler = new DeleteDocumentCommandHandler(
            _documentRepository, _fileStorage, _accessService, _unitOfWork);

        var result = await handler.Handle(new DeleteDocumentCommand(docId, callerUserId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Document not found.");
        await _documentRepository.DidNotReceive().DeleteAsync(Arg.Any<Domain.Entities.Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteDocumentCommandHandler_WhenOwner_DeletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _currentUser.UserId.Returns(userId.ToString());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);
        _profileDirectory.GetByUserIdAsync(userId).Returns(ProfileDirectoryEntry.Create(profileId, userId));

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(docId, profileId, docType);
        _documentRepository.GetByIdAsync(docId, Arg.Any<CancellationToken>()).Returns(doc);

        var handler = new DeleteDocumentCommandHandler(
            _documentRepository, _fileStorage, _accessService, _unitOfWork);

        var result = await handler.Handle(new DeleteDocumentCommand(docId, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _fileStorage.Received(1).DeleteFileAsync(doc.FilePath, Arg.Any<CancellationToken>());
        await _documentRepository.Received(1).DeleteAsync(doc, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteDocumentCommandHandler_WhenAdmin_DeletesAnyDocument()
    {
        var adminUserId = Guid.NewGuid();
        var targetProfileId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _currentUser.UserId.Returns(adminUserId.ToString());
        _currentUser.IsInRole("Admin").Returns(true);

        var docType = CreateActiveDocumentType(Guid.NewGuid());
        var doc = CreateDocument(docId, targetProfileId, docType);
        _documentRepository.GetByIdAsync(docId, Arg.Any<CancellationToken>()).Returns(doc);

        var handler = new DeleteDocumentCommandHandler(
            _documentRepository, _fileStorage, _accessService, _unitOfWork);

        var result = await handler.Handle(new DeleteDocumentCommand(docId, adminUserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _fileStorage.Received(1).DeleteFileAsync(doc.FilePath, Arg.Any<CancellationToken>());
        await _documentRepository.Received(1).DeleteAsync(doc, Arg.Any<CancellationToken>());
    }

    #endregion
}
