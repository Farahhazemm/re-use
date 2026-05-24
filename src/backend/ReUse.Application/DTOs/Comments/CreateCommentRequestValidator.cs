using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace ReUse.Application.DTOs.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required.")
            .MinimumLength(2).WithMessage("Comment must be at least 2 characters.")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
    }
}