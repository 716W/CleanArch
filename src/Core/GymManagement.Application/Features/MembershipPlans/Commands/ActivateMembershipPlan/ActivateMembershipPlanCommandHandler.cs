using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Exceptions;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.ActivateMembershipPlan;

public class ActivateMembershipPlanCommandHandler : IRequestHandler<ActivateMembershipPlanCommand, bool>
{
    private readonly IMembershipPlanRepository _repository;

    public ActivateMembershipPlanCommandHandler(IMembershipPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        ActivateMembershipPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
            throw new NotFoundException($"Membership plan with id '{request.Id}' was not found.");

        plan.IsActive = true;
        await _repository.UpdateAsync(plan, cancellationToken);

        return true;
    }
}
