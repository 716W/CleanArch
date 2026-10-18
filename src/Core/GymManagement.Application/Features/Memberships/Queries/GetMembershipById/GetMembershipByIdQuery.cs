using GymManagement.Application.Features.Memberships.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Queries.GetMembershipById;

public record GetMembershipByIdQuery(Guid Id) : IRequest<MembershipResponse>;