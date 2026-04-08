using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class LoanApplicationValidator : AbstractValidator<LoanApplication>
{
    public LoanApplicationValidator()
    {
        RuleFor(x => x.ApplicantName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LoanAmount)
            .NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.RiskRating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
