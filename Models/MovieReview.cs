using System.ComponentModel.DataAnnotations;

namespace CineScope.Models;
public class MovieReview
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    [Required] public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    [Range(1, 10)] public int Rating { get; set; }
    [Required, StringLength(1200, MinimumLength = 3)] public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = true;
}
