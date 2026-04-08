namespace Buckeye.Lending.Api.Dtos;

public class ReviewQueueResponse
{
    public int Id { get; set; }
    public string OfficerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Items — with flattened loan details, not raw entities
    public List<ReviewItemResponse> Items { get; set; } = new();

    // COMPUTED VALUES — the backend calculates these
    public int TotalItems { get; set; }
    public int HighPriorityCount { get; set; }
    public int MediumPriorityCount { get; set; }
    public int LowPriorityCount { get; set; }
}