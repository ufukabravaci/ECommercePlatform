using ECommercePlatform.MvcAdmin.DTOs;

namespace ECommercePlatform.MvcAdmin.Models.Reviews;

public class AllReviewsViewModel
{
    public PageResult<ReviewDto> Reviews { get; set; } = new();

    // Filtreler
    public bool? IsApproved { get; set; }
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;

    // İstatistikler (Sayfa içindeki verilerden hesaplanır)
    public int TotalCount => Reviews?.TotalCount ?? 0;
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public int RepliedCount { get; set; }
    public double AverageRating { get; set; }
}