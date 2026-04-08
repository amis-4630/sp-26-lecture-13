using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanTypesController : ControllerBase
{
    private readonly LendingContext _context;

    public LoanTypesController(LendingContext context)
    {
        _context = context;
    }

    /// <summary>Get all loan types.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanType>>> GetAll()
    {
        return Ok(await _context.LoanTypes.ToListAsync());
    }

    /// <summary>Get a single loan type by ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LoanType>> GetById(int id)
    {
        var loanType = await _context.LoanTypes.FindAsync(id);
        if (loanType == null)
            throw new KeyNotFoundException($"Loan type with ID {id} not found");

        return Ok(loanType);
    }
}
