using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace ReUse.Application.DTOs.Broadcast;

public class CreateBroadcastRequestValidator : AbstractValidator<CreateBroadcastRequest>
{
    public CreateBroadcastRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.TargetAudience)
            .IsInEnum()
             .WithMessage("TargetAudience is invalid");
        RuleFor(x => x.ScheduledAt)
           .GreaterThan(x => DateTime.UtcNow)
            .When(x => x.ScheduledAt.HasValue)
            .WithMessage("ScheduledAt must be in the future");
    }
}