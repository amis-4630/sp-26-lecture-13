using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class LoanTypeValidator : AbstractValidator<LoanType>
{
    public LoanTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(200);
    }
}
