using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Domain.Entities;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Queries.GetMembershipById;

public record GetMembershipByIdQuery(Guid id) : IRequest<MembershipResponse> { }