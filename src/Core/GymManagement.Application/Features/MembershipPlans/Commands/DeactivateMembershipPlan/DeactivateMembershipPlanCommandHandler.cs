using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Exceptions;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.DeactivateMembershipPlan;

public class DeactivateMembershipPlanCommandHandler : IRequestHandler<DeactivateMembershipPlanCommand, bool>
{
    private readonly IMembershipPlanRepository _repository;

    public DeactivateMembershipPlanCommandHandler(IMembershipPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        DeactivateMembershipPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
            throw new NotFoundException($"Membership plan with id '{request.Id}' was not found.");

        plan.IsActive = false;
        await _repository.UpdateAsync(plan, cancellationToken);

        return true;
    }
}
