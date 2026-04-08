using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class ApplicantValidator : AbstractValidator<Applicant>
{
    public ApplicantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Phone)
            .MaximumLength(20);
    }
}
