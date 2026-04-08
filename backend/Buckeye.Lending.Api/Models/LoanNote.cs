namespace Buckeye.Lending.Api.Models;

/// <summary>
/// A timestamped comment attached to a loan application (replaces the single Notes string).
/// </summary>
public class LoanNote
{
    public int Id { get; set; }

    public string Author { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Foreign key — each note belongs to one loan application
    public int LoanApplicationId { get; set; }

    // Navigation property
    public LoanApplication? LoanApplication { get; set; }
}
