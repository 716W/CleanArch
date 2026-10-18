using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Queries.GetMember;

public record GetMemberQuery(Guid Id) : IRequest<MemberResponse>;
