using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.UpdateMember;

public record UpdateMemberCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth
) : IRequest<MemberResponse>;