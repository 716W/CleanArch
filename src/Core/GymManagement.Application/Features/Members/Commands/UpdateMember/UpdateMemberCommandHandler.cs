using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, MemberResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public UpdateMemberCommandHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<MemberResponse> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (member is null)
            throw new NotFoundException($"Member with id '{request.Id}' was not found.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.DateOfBirth = request.DateOfBirth;

        await _memberRepository.UpdateAsync(
            member,
            cancellationToken);

        return _mapper.Map<MemberResponse>(member);
    }
}