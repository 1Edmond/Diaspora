using FluentAssertions;
using Moq;
using Procedure.Application.Handlers.Queries;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Procedure.Domain.Entities;
using Shared.Application.Models;
using Shared.Domain.Enums;
using Xunit;

namespace Procedure.UnitTests;

public class GetAdminUserProceduresQueryHandlerTests
{
    private readonly Mock<IUserProcedureRepository> _repositoryMock;
    private readonly GetAdminUserProceduresQueryHandler _handler;

    public GetAdminUserProceduresQueryHandlerTests()
    {
        _repositoryMock = new Mock<IUserProcedureRepository>();
        _handler = new GetAdminUserProceduresQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedUserProcedures_WhenProceduresExist()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var procedureId = Guid.NewGuid();
        var userProcedure = UserProcedure.Create(profileId, procedureId).Value;
        userProcedure.Start();

        var items = new List<UserProcedure> { userProcedure };
        var pagedResult = new PagedList<UserProcedure>(items, 1, 1, 20);

        _repositoryMock
            .Setup(r => r.GetAllAdminAsync(
                It.IsAny<UserProcedureStatus?>(),
                It.IsAny<Guid?>(),
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new GetAdminUserProceduresQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);

        var dto = result.Value.Items.First();
        dto.Id.Should().Be(userProcedure.Id);
        dto.ProfileId.Should().Be(profileId);
        dto.ProcedureId.Should().Be(procedureId);
        dto.Status.Should().Be(UserProcedureStatus.InProgress);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatusAndProcedureId_WhenProvided()
    {
        // Arrange
        var expectedStatus = UserProcedureStatus.Completed;
        var expectedProcedureId = Guid.NewGuid();
        var pagedResult = new PagedList<UserProcedure>(new List<UserProcedure>(), 0, 1, 10);

        _repositoryMock
            .Setup(r => r.GetAllAdminAsync(
                expectedStatus,
                expectedProcedureId,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new GetAdminUserProceduresQuery(
            Status: expectedStatus,
            ProcedureId: expectedProcedureId,
            PageNumber: 1,
            PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        _repositoryMock.Verify(r => r.GetAllAdminAsync(
            expectedStatus,
            expectedProcedureId,
            1,
            10,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedList_WhenNoProceduresFound()
    {
        // Arrange
        var emptyPaged = new PagedList<UserProcedure>(new List<UserProcedure>(), 0, 1, 20);

        _repositoryMock
            .Setup(r => r.GetAllAdminAsync(
                null,
                null,
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPaged);

        var query = new GetAdminUserProceduresQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }
}
