using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Memberships.DTOs;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Queries.GetMembershipsByMember;

public class GetMembershipsByMemberQueryHandler : IRequestHandler<GetMembershipsByMemberQuery, IReadOnlyList<MembershipResponse>>
{
    private readonly IMembershipRepository _repository;
    private readonly IMapper _mapper;

    public GetMembershipsByMemberQueryHandler(IMembershipRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    async Task<IReadOnlyList<MembershipResponse>> IRequestHandler<GetMembershipsByMemberQuery, IReadOnlyList<MembershipResponse>>.Handle(GetMembershipsByMemberQuery request, CancellationToken cancellationToken)
    {
        var memberships = await _repository.GetByMemberIdAsync(
            request.MemberId,
            cancellationToken);

        return _mapper.Map<List<MembershipResponse>>(memberships);
    }
}