using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.DeactivateMembershipPlan;

public record DeactivateMembershipPlanCommand(Guid Id) : IRequest<bool>;
