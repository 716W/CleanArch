using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Queries.GetMembers;

public record GetMembersQuery : IRequest<IReadOnlyList<MemberResponse>>;
