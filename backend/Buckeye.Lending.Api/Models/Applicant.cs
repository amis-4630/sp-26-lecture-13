namespace Buckeye.Lending.Api.Models;

/// <summary>
/// Represents a loan applicant — one applicant can have many loan applications.
/// </summary>
public class Applicant
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation — one applicant can file many loan applications
    public List<LoanApplication> LoanApplications { get; set; } = [];
}
