using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace ReUse.Application.DTOs.Reports;

public class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum().WithMessage("Invalid report target type.");

        RuleFor(x => x.TargetId)
            .NotEmpty().WithMessage("Target ID is required.");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid report reason.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}