using MediatR;

namespace GymManagement.Application.Features.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid id) : IRequest<bool> { }