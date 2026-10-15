using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Members.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Members.Queries.GetMember;

public class GetMemberQueryHandler : IRequestHandler<GetMemberQuery, MemberResponse?>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public GetMemberQueryHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<MemberResponse?> Handle(GetMemberQuery request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id, cancellationToken);
        if (member is null)
        {
            return null;
        }

        return _mapper.Map<MemberResponse>(member);
    }
}
