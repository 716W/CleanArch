using GymManagement.Domain.Entities;
using GymManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Tests;

public class MemberRepositoryTests : InfrastructureTestBase
{
    private MemberRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new MemberRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistMember_AndGetByIdAsync_ShouldRetrieveIt()
    {
        // Arrange
        var member = CreateMember(
            email: "sarah.connor@example.com",
            firstName: "Sarah",
            lastName: "Connor",
            dateOfBirth: new DateTime(1985, 2, 20),
            isActive: true);

        // Act
        var addedMember = await _repository.AddAsync(member);

        // Assert
        Assert.Equal(member.Id, addedMember.Id);

        using var cleanContext = CreateCleanContext();
        var cleanRepo = new MemberRepository(cleanContext);
        var retrieved = await cleanRepo.GetByIdAsync(member.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(member.Id, retrieved.Id);
        Assert.Equal("Sarah", retrieved.FirstName);
        Assert.Equal("Connor", retrieved.LastName);
        Assert.Equal("sarah.connor@example.com", retrieved.Email);
        Assert.Equal(new DateTime(1985, 2, 20), retrieved.DateOfBirth);
        Assert.True(retrieved.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPersistedMembers()
    {
        // Arrange
        var member1 = CreateMember("member1@example.com", firstName: "Alice");
        var member2 = CreateMember("member2@example.com", firstName: "Bob");
        await Context.Members.AddRangeAsync(member1, member2);
        await Context.SaveChangesAsync();

        // Act
        var allMembers = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, allMembers.Count);
        Assert.Contains(allMembers, m => m.Id == member1.Id && m.FirstName == "Alice");
        Assert.Contains(allMembers, m => m.Id == member2.Id && m.FirstName == "Bob");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePersistedMember()
    {
        // Arrange
        var member = CreateMember("original@example.com", firstName: "OriginalName");
        await Context.Members.AddAsync(member);
        await Context.SaveChangesAsync();

        // Act
        member.FirstName = "UpdatedName";
        member.LastName = "UpdatedLastName";
        member.IsActive = false;
        await _repository.UpdateAsync(member);

        // Assert
        using var cleanContext = CreateCleanContext();
        var cleanRepo = new MemberRepository(cleanContext);
        var updated = await cleanRepo.GetByIdAsync(member.Id);

        Assert.NotNull(updated);
        Assert.Equal("UpdatedName", updated.FirstName);
        Assert.Equal("UpdatedLastName", updated.LastName);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveMemberFromDatabase()
    {
        // Arrange
        var member = CreateMember("to.delete@example.com");
        await Context.Members.AddAsync(member);
        await Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(member);

        // Assert
        using var cleanContext = CreateCleanContext();
        var cleanRepo = new MemberRepository(cleanContext);
        var deleted = await cleanRepo.GetByIdAsync(member.Id);
        var all = await cleanRepo.GetAllAsync();

        Assert.Null(deleted);
        Assert.DoesNotContain(all, m => m.Id == member.Id);
    }

    [Fact]
    public async Task AddAsync_WhenDuplicateEmailIsInserted_ShouldThrowDbUpdateExceptionWrappingSqliteConstraintException()
    {
        // Arrange
        var firstMember = CreateMember(email: "unique@example.com", firstName: "First");
        var secondMemberWithSameEmail = CreateMember(email: "unique@example.com", firstName: "Second");

        await _repository.AddAsync(firstMember);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => _repository.AddAsync(secondMemberWithSameEmail));

        Assert.NotNull(ex.InnerException);
        Assert.IsType<SqliteException>(ex.InnerException);

        var sqliteException = (SqliteException)ex.InnerException;
        Assert.Equal(19, sqliteException.SqliteErrorCode); // SQLITE_CONSTRAINT
        Assert.Contains("UNIQUE constraint failed: Members.Email", sqliteException.Message);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        var member = CreateMember("exists@example.com");
        await Context.Members.AddAsync(member);
        await Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsByEmailAsync("exists@example.com");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsByEmailAsync("doesnotexist@example.com");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailBelongsToExcludedMemberId_ShouldReturnFalse()
    {
        // Arrange
        var member = CreateMember("self@example.com");
        await Context.Members.AddAsync(member);
        await Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsByEmailAsync("self@example.com", excludeMemberId: member.Id);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailBelongsToAnotherMemberAndExcludedIdProvided_ShouldReturnTrue()
    {
        // Arrange
        var memberA = CreateMember("other@example.com");
        var memberB = CreateMember("me@example.com");
        await Context.Members.AddRangeAsync(memberA, memberB);
        await Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsByEmailAsync("other@example.com", excludeMemberId: memberB.Id);

        // Assert
        Assert.True(exists);
    }
}
