using FluentValidation;
using Buckeye.Lending.Api.Dtos;
using Buckeye.Lending.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Buckeye.Lending.Api.Validators;

public class AddToQueueRequestValidator : AbstractValidator<AddToQueueRequest>
{
    public AddToQueueRequestValidator(LendingContext context)
    {
        RuleFor(x => x.LoanApplicationId)
            .GreaterThan(0)
            .WithMessage("LoanApplicationId must be a positive integer.")
            .MustAsync(async (id, cancellation) =>
            {
                return await context.LoanApplications
                    .AnyAsync(l => l.Id == id, cancellation);
            })
            .WithMessage("Loan application not found.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5)
            .WithMessage("Priority must be between 1 and 5.");
    }
}