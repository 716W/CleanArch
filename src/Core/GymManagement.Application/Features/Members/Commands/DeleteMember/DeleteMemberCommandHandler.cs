using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Exceptions;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, bool>
{
    private readonly IMemberRepository _memberRepository;

    public DeleteMemberCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<bool> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id, cancellationToken);

        if (member is null)
            throw new NotFoundException($"Member with id '{request.Id}' was not found.");

        await _memberRepository.DeleteAsync(member, cancellationToken);
        return true;
    }
}