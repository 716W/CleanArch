using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Domain.Entities;
using MediatR;

namespace GymManagement.Application.Features.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, MemberResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public CreateMemberCommandHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<MemberResponse> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = _mapper.Map<Member>(request);
        member.Id = Guid.NewGuid();
        member.IsActive = true;
        member.JoinedDate = DateTime.UtcNow;

        var createdMember = await _memberRepository.AddAsync(member, cancellationToken);

        return _mapper.Map<MemberResponse>(createdMember);
    }
}
