using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Entities;

public class Membership
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid MembershipPlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    public string PaymentTransactionId { get; set; } = string.Empty;

    public Member Member { get; set; } = null!;

    public MembershipPlan MembershipPlan { get; set; } = null!;
}