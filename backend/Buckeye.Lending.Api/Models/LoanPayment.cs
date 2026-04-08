using System.ComponentModel.DataAnnotations.Schema;

namespace Buckeye.Lending.Api.Models;

/// <summary>
/// Tracks a single payment made against an approved loan application.
/// </summary>
public class LoanPayment
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string Method { get; set; } = string.Empty; // e.g. "ACH", "Check", "Wire"

    // Foreign key — each payment belongs to one loan application
    public int LoanApplicationId { get; set; }

    // Navigation property
    public LoanApplication? LoanApplication { get; set; }
}
