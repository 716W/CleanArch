using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Memberships.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Queries.GetMembershipsByMember;

public class GetMembershipsByMemberQueryHandler : IRequestHandler<GetMembershipsByMemberQuery, IReadOnlyList<MembershipResponse>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMapper _mapper;

    public GetMembershipsByMemberQueryHandler(
        IMemberRepository memberRepository,
        IMembershipRepository membershipRepository,
        IMapper mapper)
    {
        _memberRepository = memberRepository;
        _membershipRepository = membershipRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MembershipResponse>> Handle(
        GetMembershipsByMemberQuery request,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
        {
            throw new NotFoundException($"Member with id '{request.MemberId}' was not found.");
        }

        var memberships = await _membershipRepository.GetByMemberIdAsync(
            request.MemberId,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<MembershipResponse>>(memberships);
    }
}