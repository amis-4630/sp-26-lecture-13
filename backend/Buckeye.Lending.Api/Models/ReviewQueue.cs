namespace Buckeye.Lending.Api.Models;

public class ReviewQueue
{
    public int Id { get; set; }

    public string OfficerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — one queue has many items
    public ICollection<ReviewItem> Items { get; set; } = new List<ReviewItem>();
}