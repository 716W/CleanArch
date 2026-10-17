using System.Data.Common;
using AutoMapper;
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Commands.CreateMemberships;

public class CreateMembershipCommandHandler : IRequestHandler<CreateMembershipCommand, MembershipResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipPlanRepository _planRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMapper _mapper;

    public CreateMembershipCommandHandler(IMemberRepository memberRepository, IMembershipPlanRepository planRepository, IMembershipRepository membershipRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _planRepository = planRepository;
        _membershipRepository = membershipRepository;
        _mapper = mapper;
    }

    public async Task<MembershipResponse> Handle(CreateMembershipCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(
         request.MemberId,
         cancellationToken);

        if (member is null)
            throw new KeyNotFoundException("Member not found.");

        if (!member.IsActive)
            throw new InvalidOperationException("Member is inactive.");

        var plan = await _planRepository.GetByIdAsync(
            request.MembershipPlanId,
            cancellationToken);

        if (plan is null)
            throw new KeyNotFoundException("Membership plan not found.");

        if (!plan.IsActive)
            throw new InvalidOperationException("Membership plan is inactive.");

        if (plan.DurationInDays <= 0 || plan.Price < 0)
            throw new InvalidOperationException("Membership plan configuration is invalid.");

        var startDate = request.StartDate.Date;

        var hasConflict =
            await _membershipRepository.HasConflictingMembershipAsync(
                member.Id,
                startDate,
                cancellationToken);

        if (hasConflict)
            throw new InvalidOperationException(
                "The member already has a conflicting membership.");

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = startDate,
            EndDate = startDate.AddDays(plan.DurationInDays),
            Status = MembershipStatus.Active,
            PaymentTransactionId = request.PaymentTransactionId.Trim()
        };

        var createdMembership =
            await _membershipRepository.AddAsync(
                    membership,
                    cancellationToken
            );

        return _mapper.Map<MembershipResponse>(createdMembership);
    }

}