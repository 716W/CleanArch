using FluentValidation;
using GymManagement.Application.Features.Members.Commands.CreateMember;

namespace GymManagement.Application.Features.Memberships.Commands.CreateMemberships;

public class CreateMembershipValidator : AbstractValidator<CreateMembershipCommand>
{
    public CreateMembershipValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty();

        RuleFor(x => x.MembershipPlanId)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.PaymentTransactionId)
            .NotEmpty()
            .MaximumLength(200);


    }
}