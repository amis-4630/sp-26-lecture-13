using System.ComponentModel.DataAnnotations.Schema;

namespace Buckeye.Lending.Api.Models;

public class LoanApplication
{
    public int Id { get; set; }

    public string ApplicantName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal LoanAmount { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal AnnualIncome { get; set; }

    public string Status { get; set; } = "Pending";

    public int RiskRating { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    // Foreign key — which applicant filed this application
    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    // Foreign key — what type of loan (replaces the old string LoanType)
    public int LoanTypeId { get; set; }
    public LoanType? LoanType { get; set; }

    // Navigation — one application can have many payments and notes
    public List<LoanPayment> Payments { get; set; } = [];
    public List<LoanNote> LoanNotes { get; set; } = [];
}
