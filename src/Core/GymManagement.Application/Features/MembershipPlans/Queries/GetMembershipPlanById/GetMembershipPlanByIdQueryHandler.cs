using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;

public class GetMembershipPlanByIdQueryHandler : IRequestHandler<GetMembershipPlanByIdQuery, MembershipPlanResponse>
{
    private readonly IMembershipPlanRepository _repository;
    private readonly IMapper _mapper;

    public GetMembershipPlanByIdQueryHandler(IMembershipPlanRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MembershipPlanResponse> Handle(
        GetMembershipPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
            throw new KeyNotFoundException($"Membership plan with id '{request.Id}' was not found.");

        return _mapper.Map<MembershipPlanResponse>(plan);
    }
}
