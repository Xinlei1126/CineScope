using System.ComponentModel.DataAnnotations;

namespace CineScope.Models;
public class Movie
{
    public int Id { get; set; }
    [Required, StringLength(160)] public string Title { get; set; } = "";
    [Required, StringLength(60)] public string Genre { get; set; } = "";
    [Range(1888, 2100)] public int ReleaseYear { get; set; }
    [Range(0, 10)] public decimal Rating { get; set; }
    [Range(1, 1000)] public int Duration { get; set; }
    [Url, StringLength(500)] public string? PosterUrl { get; set; }
    [StringLength(4000)] public string? Description { get; set; }
    public ICollection<MovieReview> Reviews { get; set; } = new List<MovieReview>();
    public ICollection<MovieFavorite> Favorites { get; set; } = new List<MovieFavorite>();
}
