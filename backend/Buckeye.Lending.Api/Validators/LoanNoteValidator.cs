using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class LoanNoteValidator : AbstractValidator<LoanNote>
{
    public LoanNoteValidator()
    {
        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
