namespace CineScope.Models;
public class MovieFavorite
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
