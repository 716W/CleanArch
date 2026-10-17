using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public record CreateMembershipPlanCommand(
    string Name,
    int DurationInDays,
    decimal Price,
    int MaxFreezeDays,
    int DisplayOrder
) : IRequest<MembershipPlanResponse>;
