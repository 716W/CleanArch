namespace GymManagement.Application.Features.Memberships.DTOs;

public class MembershipResponse
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid MembershipPlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;
}