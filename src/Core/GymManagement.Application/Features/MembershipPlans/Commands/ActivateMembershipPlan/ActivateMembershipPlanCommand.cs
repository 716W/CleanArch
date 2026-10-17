using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.ActivateMembershipPlan;

public record ActivateMembershipPlanCommand(Guid Id) : IRequest<bool>;
