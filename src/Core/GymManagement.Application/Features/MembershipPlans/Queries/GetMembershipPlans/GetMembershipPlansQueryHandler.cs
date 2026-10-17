using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public class GetMembershipPlansQueryHandler
    : IRequestHandler<GetMembershipPlansQuery, IReadOnlyList<MembershipPlanResponse>>
{
    private readonly IMembershipPlanRepository _repository;
    private readonly IMapper _mapper;

    public GetMembershipPlansQueryHandler(IMembershipPlanRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MembershipPlanResponse>> Handle(
        GetMembershipPlansQuery request,
        CancellationToken cancellationToken)
    {
        var plans = await _repository.GetAllAsync(request.ActiveOnly, cancellationToken);

        return _mapper.Map<IReadOnlyList<MembershipPlanResponse>>(plans);
    }
}
