using FluentValidation;

using ReUse.Application.DTOs.Dashboard;
using ReUse.Application.Enums;

namespace ReUse.Application.Validators.Dashboard;

public class DashboardQueryValidator : AbstractValidator<DashboardQuery>
{
    public DashboardQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(Enum.IsDefined<DashboardPeriod>)
            .WithMessage("Invalid dashboard period.");
    }
}