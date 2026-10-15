using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.CreateMember;

public record CreateMemberCommand(
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth) : IRequest<MemberResponse>;
