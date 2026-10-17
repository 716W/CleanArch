using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;

public record GetMembershipPlanByIdQuery(Guid Id) : IRequest<MembershipPlanResponse>;
