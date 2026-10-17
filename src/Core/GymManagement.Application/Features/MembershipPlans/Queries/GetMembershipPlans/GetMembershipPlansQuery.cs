using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public record GetMembershipPlansQuery(bool? ActiveOnly = null) : IRequest<IReadOnlyList<MembershipPlanResponse>>;
