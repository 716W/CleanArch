using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Members.Commands.UpdateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.Members;

public class UpdateMemberCommandHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;
    private readonly IMapper _mapper;

    public UpdateMemberCommandHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingMember_ReturnsUpdatedMemberResponse()
    {
        // Arrange
        var handler = new UpdateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var existingMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var command = new UpdateMemberCommand(
            existingMemberId,
            "Alexander",
            "Smith",
            "alex.smith@example.com",
            new DateTime(1999, 12, 31));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingMemberId, result.Id);
        Assert.Equal("Alexander", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal("alex.smith@example.com", result.Email);
        Assert.Equal(new DateTime(1999, 12, 31), result.DateOfBirth);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingMember_CallsRepositoryUpdateAsyncWithUpdatedEntityAndCancellationToken()
    {
        // Arrange
        var handler = new UpdateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var existingMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var command = new UpdateMemberCommand(
            existingMemberId,
            "Alexander",
            "Smith",
            "alex.smith@example.com",
            new DateTime(1999, 12, 31));

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _mockMemberRepository.Mock.Verify(
            repo => repo.GetByIdAsync(existingMemberId, cancellationToken),
            Times.Once);

        _mockMemberRepository.Mock.Verify(
            repo => repo.UpdateAsync(
                It.Is<Member>(m =>
                    m.Id == existingMemberId &&
                    m.FirstName == command.FirstName &&
                    m.LastName == command.LastName &&
                    m.Email == command.Email &&
                    m.DateOfBirth == command.DateOfBirth),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new UpdateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var command = new UpdateMemberCommand(
            nonExistentId,
            "Non",
            "Existent",
            "non.existent@example.com",
            new DateTime(1990, 1, 1));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Member with id '{nonExistentId}' was not found.", exception.Message);

        _mockMemberRepository.Mock.Verify(
            repo => repo.UpdateAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateEmailBelongingToAnotherMember_ThrowsConflictException()
    {
        // Arrange
        var handler = new UpdateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var anotherMemberEmail = "inactive@example.com"; // Belongs to 22222222-...

        var command = new UpdateMemberCommand(
            memberId,
            "Ali",
            "Updated",
            anotherMemberEmail,
            new DateTime(2000, 1, 1));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"A member with email '{anotherMemberEmail}' already exists.", exception.Message);

        _mockMemberRepository.Mock.Verify(
            repo => repo.UpdateAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RetainingOwnEmail_ShouldSucceed()
    {
        // Arrange
        var handler = new UpdateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var ownEmail = "ali@example.com";

        var command = new UpdateMemberCommand(
            memberId,
            "AliNewName",
            "Test",
            ownEmail,
            new DateTime(2000, 1, 1));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.Id);
        Assert.Equal("AliNewName", result.FirstName);
        Assert.Equal(ownEmail, result.Email);

        _mockMemberRepository.Mock.Verify(
            repo => repo.UpdateAsync(
                It.Is<Member>(m => m.Id == memberId && m.Email == ownEmail && m.FirstName == "AliNewName"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
