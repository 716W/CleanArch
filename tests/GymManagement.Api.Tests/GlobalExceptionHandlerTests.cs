using System.Net;
using System.Text.Json;
using GymManagement.Api.Middlewares;
using GymManagement.Domain.Entities;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GymManagement.Api.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenDbUpdateExceptionIsUniqueConstraintViolation_ReturnsConflict409ProblemDetails()
    {
        // Arrange: Generate a genuine EF Core + SQLite unique constraint DbUpdateException
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseSqlite(connection)
            .Options;

        using var dbContext = new GymDbContext(options);
        await dbContext.Database.MigrateAsync();

        var member1 = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "User",
            Email = "race.condition@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };

        var member2 = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "Second",
            LastName = "User",
            Email = "race.condition@example.com",
            DateOfBirth = new DateTime(1992, 2, 2),
            IsActive = true
        };

        await dbContext.Members.AddAsync(member1);
        await dbContext.SaveChangesAsync();

        await dbContext.Members.AddAsync(member2);
        var genuineUniqueConstraintException = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());

        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var httpContext = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        httpContext.Response.Body = responseStream;

        // Act: Execute exception handler with genuine race-condition DbUpdateException
        var handled = await handler.TryHandleAsync(httpContext, genuineUniqueConstraintException, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal((int)HttpStatusCode.Conflict, httpContext.Response.StatusCode);
        Assert.Contains("application/json", httpContext.Response.ContentType);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Equal("A resource with the specified unique value already exists.", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_WhenDbUpdateExceptionIsNotUniqueConstraintViolation_ReturnsInternalServerError500()
    {
        // Arrange: An unrelated DbUpdateException (e.g. disk failure or timeout)
        var unrelatedDbException = new DbUpdateException("General database error", new Exception("Disk failure"));
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var httpContext = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        httpContext.Response.Body = responseStream;

        // Act
        var handled = await handler.TryHandleAsync(httpContext, unrelatedDbException, CancellationToken.None);

        // Assert: Unrelated database failures must remain 500 server errors
        Assert.True(handled);
        Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
        Assert.Contains("application/json", httpContext.Response.ContentType);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
    }
}
