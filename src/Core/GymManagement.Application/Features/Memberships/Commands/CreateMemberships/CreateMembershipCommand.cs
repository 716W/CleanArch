using GymManagement.Application.Features.Memberships.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Commands.CreateMemberships;

public class CreateMembershipCommand : IRequest<MembershipResponse>
{
    public Guid MemberId { get; set; }
    public Guid MembershipPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public string PaymentTransactionId { get; set; } = String.Empty;
}