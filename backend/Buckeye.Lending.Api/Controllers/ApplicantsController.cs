using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Models;
using Buckeye.Lending.Api.Validators;

namespace Buckeye.Lending.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicantsController : ControllerBase
{
    private readonly LendingContext _context;
    private readonly IValidator<Applicant> _validator;

    public ApplicantsController(LendingContext context, IValidator<Applicant> validator)
    {
        _context = context;
        _validator = validator;
    }

    /// <summary>Get all applicants.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Applicant>>> GetAll()
    {
        return Ok(await _context.Applicants.ToListAsync());
    }

    /// <summary>Get a single applicant by ID, including their loan applications.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Applicant>> GetById(int id)
    {
        var applicant = await _context.Applicants
            .Include(a => a.LoanApplications)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (applicant == null)
            throw new KeyNotFoundException($"Applicant with ID {id} not found");

        return Ok(applicant);
    }

    /// <summary>Create a new applicant.</summary>
    [HttpPost]
    public async Task<ActionResult<Applicant>> Create(Applicant applicant)
    {
        var result = await _validator.ValidateAsync(applicant);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        applicant.CreatedDate = DateTime.UtcNow;

        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = applicant.Id }, applicant);
    }
}
