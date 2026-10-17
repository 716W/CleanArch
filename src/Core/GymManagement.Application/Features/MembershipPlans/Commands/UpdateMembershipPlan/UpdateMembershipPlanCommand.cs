using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;

public record UpdateMembershipPlanCommand(
    Guid Id,
    string Name,
    int DurationInDays,
    decimal Price,
    int MaxFreezeDays,
    int DisplayOrder
) : IRequest<MembershipPlanResponse?>;
