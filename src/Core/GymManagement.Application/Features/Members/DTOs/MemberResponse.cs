namespace GymManagement.Application.Features.Members.DTOs;

public record MemberResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public bool IsActive { get; init; }
    public DateTime JoinedDate { get; init; }
}
