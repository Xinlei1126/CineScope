using CineScope.Data;
using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Controllers;
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, string? genre)
    {
        var query = db.Movies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(m => m.Title.Contains(search));
        if (!string.IsNullOrWhiteSpace(genre)) query = query.Where(m => m.Genre == genre);
        var model = new MovieIndexViewModel {
            Movies = await query.OrderByDescending(m => m.Rating).ToListAsync(),
            Genres = await db.Movies.Select(m => m.Genre).Distinct().OrderBy(g => g).ToListAsync(), Search = search, Genre = genre
        };
        return View(model);
    }
    public IActionResult Error() => View();
}
