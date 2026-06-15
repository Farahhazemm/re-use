using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Reports;

public class ReviewReportRequestValidator : AbstractValidator<ReviewReportRequest>
{
    public ReviewReportRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid report status.")
            .NotEqual(ReportStatus.Pending).WithMessage("Status must be a review decision.");

        RuleFor(x => x.ReviewNotes)
            .MaximumLength(1000).WithMessage("Review notes must not exceed 1000 characters.")
            .When(x => x.ReviewNotes is not null);
    }
}