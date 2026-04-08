namespace Buckeye.Lending.Api.Dtos;

public class ReviewItemResponse
{
    public int Id { get; set; }
    public int Priority { get; set; }
    public string? Notes { get; set; }

    // Flattened from the LoanApplication navigation property
    public int LoanApplicationId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}