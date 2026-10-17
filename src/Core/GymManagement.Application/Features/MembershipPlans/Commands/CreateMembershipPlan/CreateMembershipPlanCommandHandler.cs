using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Domain.Entities;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public class CreateMembershipPlanCommandHandler : IRequestHandler<CreateMembershipPlanCommand, MembershipPlanResponse>
{
    private readonly IMembershipPlanRepository _repository;
    private readonly IMapper _mapper;

    public CreateMembershipPlanCommandHandler(IMembershipPlanRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MembershipPlanResponse> Handle(
        CreateMembershipPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = new MembershipPlan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DurationInDays = request.DurationInDays,
            Price = request.Price,
            MaxFreezeDays = request.MaxFreezeDays,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        var created = await _repository.AddAsync(plan, cancellationToken);

        return _mapper.Map<MembershipPlanResponse>(created);
    }
}
