using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.Members;

public class CreateMemberCommandHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;
    private readonly IMapper _mapper;

    public CreateMemberCommandHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsMemberResponse()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var command = new CreateMemberCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1995, 5, 20));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.FirstName, result.FirstName);
        Assert.Equal(command.LastName, result.LastName);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.DateOfBirth, result.DateOfBirth);
        Assert.True(result.IsActive);
        Assert.True((DateTime.UtcNow - result.JoinedDate).TotalMinutes < 1);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsyncWithExpectedMemberAndCancellationToken()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var command = new CreateMemberCommand(
            "Jane",
            "Smith",
            "jane.smith@example.com",
            new DateTime(1992, 8, 15));

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        _mockMemberRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Member>(m =>
                    m.FirstName == command.FirstName &&
                    m.LastName == command.LastName &&
                    m.Email == command.Email &&
                    m.DateOfBirth == command.DateOfBirth &&
                    m.IsActive &&
                    m.Id != Guid.Empty),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange: "ali@example.com" already exists in MockMemberRepository
        var handler = new CreateMemberCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var command = new CreateMemberCommand(
            "Ali",
            "Duplicate",
            "ali@example.com",
            new DateTime(1995, 1, 1));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("A member with email 'ali@example.com' already exists.", exception.Message);

        _mockMemberRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
