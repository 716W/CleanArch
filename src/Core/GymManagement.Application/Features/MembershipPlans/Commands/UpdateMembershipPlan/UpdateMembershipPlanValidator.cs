using FluentValidation;

namespace GymManagement.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;

public class UpdateMembershipPlanValidator : AbstractValidator<UpdateMembershipPlanCommand>
{
    private static readonly int[] AllowedDurations = [1, 7, 14, 30, 90, 180, 365];

    public UpdateMembershipPlanValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.DurationInDays)
            .Must(d => AllowedDurations.Contains(d))
            .WithMessage($"DurationInDays must be one of: {string.Join(", ", AllowedDurations)}.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Price must be greater than or equal to zero.");

        RuleFor(x => x.MaxFreezeDays)
            .GreaterThanOrEqualTo(0).WithMessage("MaxFreezeDays must be greater than or equal to zero.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be greater than or equal to zero.");
    }
}
