using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(
    IMemberRepository memberRepository
) : IRequestHandler<UpdateMemberCommand, MemberResponse?>
{
    public async Task<MemberResponse?> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(
            request.id,
            cancellationToken
        );

        if (member is null)
            return null;

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.DateOfBirth = request.DateOfBirth;

        await memberRepository.UpdateAsync(
            member,
            cancellationToken
        );

        return new MemberResponse
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email,
            DateOfBirth = member.DateOfBirth,
            IsActive = member.IsActive,
            JoinedDate = member.JoinedDate
        };
    }
}