using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;

public class UpdateMembershipPlanCommandHandler : IRequestHandler<UpdateMembershipPlanCommand, MembershipPlanResponse>
{
    private readonly IMembershipPlanRepository _repository;
    private readonly IMapper _mapper;

    public UpdateMembershipPlanCommandHandler(IMembershipPlanRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MembershipPlanResponse> Handle(
        UpdateMembershipPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
            throw new NotFoundException($"Membership plan with id '{request.Id}' was not found.");

        // IsActive is deliberately not mutated here; activation state is managed
        // exclusively by ActivateMembershipPlanCommand / DeactivateMembershipPlanCommand.
        plan.Name = request.Name;
        plan.DurationInDays = request.DurationInDays;
        plan.Price = request.Price;
        plan.MaxFreezeDays = request.MaxFreezeDays;
        plan.DisplayOrder = request.DisplayOrder;

        await _repository.UpdateAsync(plan, cancellationToken);

        return _mapper.Map<MembershipPlanResponse>(plan);
    }
}
