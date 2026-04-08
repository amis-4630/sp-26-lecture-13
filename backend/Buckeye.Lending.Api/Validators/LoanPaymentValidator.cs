using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class LoanPaymentValidator : AbstractValidator<LoanPayment>
{
    public LoanPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty();

        RuleFor(x => x.Method)
            .NotEmpty()
            .MaximumLength(30);
    }
}
