namespace GymManagement.Application.Features.MembershipPlans.DTOs;

public record MembershipPlanResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int DurationInDays { get; init; }
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
    public int MaxFreezeDays { get; init; }
    public int DisplayOrder { get; init; }
}
