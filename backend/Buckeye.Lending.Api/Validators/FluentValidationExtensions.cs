using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buckeye.Lending.Api.Validators;

public static class FluentValidationExtensions
{
    /// <summary>
    /// Copies FluentValidation errors into ASP.NET ModelState so controllers
    /// can return ValidationProblem() with structured RFC 7807 error details.
    /// </summary>
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
