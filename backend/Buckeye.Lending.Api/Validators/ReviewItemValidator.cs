using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class ReviewItemValidator : AbstractValidator<ReviewItem>
{
    public ReviewItemValidator()
    {
        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5);
    }
}
