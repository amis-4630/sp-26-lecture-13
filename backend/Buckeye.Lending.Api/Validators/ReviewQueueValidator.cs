using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class ReviewQueueValidator : AbstractValidator<ReviewQueue>
{
    public ReviewQueueValidator()
    {
        RuleFor(x => x.OfficerId)
            .NotEmpty();
    }
}
