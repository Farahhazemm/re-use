using FluentValidation;

namespace ReUse.Application.DTOs.Broadcast;

public class UpdateBroadcastRequestValidator : AbstractValidator<UpdateBroadcastRequest>
{
    public UpdateBroadcastRequestValidator()
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