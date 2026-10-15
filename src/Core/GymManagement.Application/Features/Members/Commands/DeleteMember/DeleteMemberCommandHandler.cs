using GymManagement.Application.Contracts.Persistence;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler(IMemberRepository memberRepository) : IRequestHandler<DeleteMemberCommand, bool>
{
    public async Task<bool> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.id, cancellationToken);

        if (member is null)
            return false;

        await memberRepository.DeleteAsync(member, cancellationToken);
        return true;
    }
}