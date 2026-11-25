using CineScope.Models;
namespace CineScope.ViewModels;
public class MovieIndexViewModel
{
    public IReadOnlyList<Movie> Movies { get; set; } = Array.Empty<Movie>();
    public IReadOnlyList<string> Genres { get; set; } = Array.Empty<string>();
    public string? Search { get; set; }
    public string? Genre { get; set; }
}
