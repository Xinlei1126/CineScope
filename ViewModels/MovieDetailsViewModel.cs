using CineScope.Models;
namespace CineScope.ViewModels;
public class MovieDetailsViewModel
{
    public Movie Movie { get; set; } = null!;
    public IReadOnlyList<MovieReview> Reviews { get; set; } = Array.Empty<MovieReview>();
    public bool IsFavorite { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public MovieReview NewReview { get; set; } = new();
}
