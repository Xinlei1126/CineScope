using System.Security.Claims;
using CineScope.Data;
using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Controllers;
public class MoviesController(ApplicationDbContext db) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var movie = await db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null) return NotFound();
        var reviews = await db.MovieReviews.Include(r => r.User).Where(r => r.MovieId == id && r.IsApproved).OrderByDescending(r => r.CreatedAt).AsNoTracking().ToListAsync();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(new MovieDetailsViewModel {
            Movie = movie, Reviews = reviews,
            AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating), ReviewCount = reviews.Count,
            IsFavorite = userId is not null && await db.MovieFavorites.AnyAsync(f => f.MovieId == id && f.UserId == userId)
        });
    }

    [HttpPost, Authorize(Roles = "Member,Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id, string? returnUrl)
    {
        if (!await db.Movies.AnyAsync(m => m.Id == id)) return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var favorite = await db.MovieFavorites.FindAsync(userId, id);
        if (favorite is null) db.MovieFavorites.Add(new MovieFavorite { UserId = userId, MovieId = id });
        else db.MovieFavorites.Remove(favorite);
        await db.SaveChangesAsync();
        return LocalRedirect(returnUrl ?? Url.Action(nameof(Details), new { id })!);
    }

    [HttpPost, Authorize(Roles = "Member,Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, MovieReviewInputViewModel input)
    {
        if (!await db.Movies.AnyAsync(m => m.Id == id)) return NotFound();
        if (!ModelState.IsValid) { TempData["Error"] = "Please enter a review and a rating from 1 to 10."; return RedirectToAction(nameof(Details), new { id }); }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await db.MovieReviews.SingleOrDefaultAsync(r => r.MovieId == id && r.UserId == userId);
        if (existing is null) db.MovieReviews.Add(new MovieReview { MovieId = id, UserId = userId, Rating = input.Rating, Body = input.Body.Trim() });
        else { existing.Rating = input.Rating; existing.Body = input.Body.Trim(); existing.CreatedAt = DateTime.UtcNow; existing.IsApproved = true; }
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Member,Admin")]
    public async Task<IActionResult> Favorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var movies = await db.MovieFavorites.Where(f => f.UserId == userId).Select(f => f.Movie).AsNoTracking().ToListAsync();
        return View(movies);
    }
}
