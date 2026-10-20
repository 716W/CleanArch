using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Members.Commands.DeleteMember;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.Members;

public class DeleteMemberCommandHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;

    public DeleteMemberCommandHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();
    }

    [Fact]
    public async Task Handle_ExistingMember_DeletesMemberAndReturnsTrue()
    {
        // Arrange
        var handler = new DeleteMemberCommandHandler(_mockMemberRepository.Mock.Object);
        var existingMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var command = new DeleteMemberCommand(existingMemberId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _mockMemberRepository.Mock.Verify(
            repo => repo.GetByIdAsync(existingMemberId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMemberRepository.Mock.Verify(
            repo => repo.DeleteAsync(
                It.Is<Member>(m => m.Id == existingMemberId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify member was removed from repository
        var deletedMember = await _mockMemberRepository.Mock.Object.GetByIdAsync(existingMemberId);
        Assert.Null(deletedMember);
    }

    [Fact]
    public async Task Handle_ExistingMember_PassesCancellationTokenToRepository()
    {
        // Arrange
        var handler = new DeleteMemberCommandHandler(_mockMemberRepository.Mock.Object);
        var existingMemberId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var command = new DeleteMemberCommand(existingMemberId);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result);

        _mockMemberRepository.Mock.Verify(
            repo => repo.GetByIdAsync(existingMemberId, cancellationToken),
            Times.Once);

        _mockMemberRepository.Mock.Verify(
            repo => repo.DeleteAsync(It.Is<Member>(m => m.Id == existingMemberId), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new DeleteMemberCommandHandler(_mockMemberRepository.Mock.Object);
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var command = new DeleteMemberCommand(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Member with id '{nonExistentId}' was not found.", exception.Message);

        _mockMemberRepository.Mock.Verify(
            repo => repo.DeleteAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
