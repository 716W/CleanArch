using AutoMapper;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Features.Members.Queries.GetMembers;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.Members;

public class GetMembersQueryHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;
    private readonly IMapper _mapper;

    public GetMembersQueryHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WhenMembersExist_ReturnsMappedMemberResponseList()
    {
        // Arrange
        var handler = new GetMembersQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var query = new GetMembersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var first = result.First(m => m.Id == Guid.Parse("11111111-1111-1111-1111-111111111111"));
        Assert.Equal("Ali", first.FirstName);
        Assert.Equal("Test", first.LastName);
        Assert.Equal("ali@example.com", first.Email);
        Assert.Equal(new DateTime(2000, 1, 1), first.DateOfBirth);
        Assert.True(first.IsActive);

        var second = result.First(m => m.Id == Guid.Parse("22222222-2222-2222-2222-222222222222"));
        Assert.Equal("Inactive", second.FirstName);
        Assert.Equal("Member", second.LastName);
        Assert.Equal("inactive@example.com", second.Email);
        Assert.Equal(new DateTime(1998, 1, 1), second.DateOfBirth);
        Assert.False(second.IsActive);
    }

    [Fact]
    public async Task Handle_WhenNoMembersExist_ReturnsEmptyList()
    {
        // Arrange
        _mockMemberRepository.Mock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Member>());

        var handler = new GetMembersQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var query = new GetMembersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ValidQuery_PassesCancellationTokenToRepository()
    {
        // Arrange
        var handler = new GetMembersQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var query = new GetMembersQuery();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMemberRepository.Mock.Verify(
            repo => repo.GetAllAsync(cancellationToken),
            Times.Once);
    }
}
