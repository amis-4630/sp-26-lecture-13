using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Models;
using Buckeye.Lending.Api.Validators;

namespace Buckeye.Lending.Api.Controllers;

[ApiController]
[Route("api/loanapplications/{loanApplicationId}/[controller]")]
public class NotesController : ControllerBase
{
    private readonly LendingContext _context;
    private readonly IValidator<LoanNote> _validator;

    public NotesController(LendingContext context, IValidator<LoanNote> validator)
    {
        _context = context;
        _validator = validator;
    }

    /// <summary>Get all notes for a loan application.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanNote>>> GetAll(int loanApplicationId)
    {
        var notes = await _context.LoanNotes
            .Where(n => n.LoanApplicationId == loanApplicationId)
            .OrderBy(n => n.CreatedDate)
            .ToListAsync();

        return Ok(notes);
    }

    /// <summary>Add a note to a loan application.</summary>
    [HttpPost]
    public async Task<ActionResult<LoanNote>> Create(int loanApplicationId, LoanNote note)
    {
        var app = await _context.LoanApplications.FindAsync(loanApplicationId);
        if (app == null)
            throw new KeyNotFoundException($"Loan application with ID {loanApplicationId} not found");

        var result = await _validator.ValidateAsync(note);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        note.LoanApplicationId = loanApplicationId;
        note.CreatedDate = DateTime.UtcNow;

        _context.LoanNotes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { loanApplicationId }, note);
    }
}
