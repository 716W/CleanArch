using GymManagement.Application.Features.Memberships.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Queries.GetMembershipsByMember;

public record GetMembershipsByMemberQuery(Guid MemberId)
    : IRequest<IReadOnlyList<MembershipResponse>>;