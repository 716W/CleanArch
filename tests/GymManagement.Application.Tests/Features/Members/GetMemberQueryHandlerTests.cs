using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Features.Members.Queries.GetMember;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using Moq;

namespace GymManagement.Application.Tests.Features.Members;

public class GetMemberQueryHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;
    private readonly IMapper _mapper;

    public GetMemberQueryHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingMember_ReturnsMappedMemberResponse()
    {
        // Arrange
        var handler = new GetMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var existingMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var query = new GetMemberQuery(existingMemberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingMemberId, result.Id);
        Assert.Equal("Ali", result.FirstName);
        Assert.Equal("Test", result.LastName);
        Assert.Equal("ali@example.com", result.Email);
        Assert.Equal(new DateTime(2000, 1, 1), result.DateOfBirth);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingMember_PassesCancellationTokenToRepository()
    {
        // Arrange
        var handler = new GetMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var existingMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var query = new GetMemberQuery(existingMemberId);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMemberRepository.Mock.Verify(
            repo => repo.GetByIdAsync(existingMemberId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mapper);

        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var query = new GetMemberQuery(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Member with id '{nonExistentId}' was not found.", exception.Message);
    }
}
