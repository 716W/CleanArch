using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, IReadOnlyList<MemberResponse>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public GetMembersQueryHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MemberResponse>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<MemberResponse>>(members);
    }
}
