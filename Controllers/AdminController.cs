using CineScope.Data;
using CineScope.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Controllers;
[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Movies.AsNoTracking().OrderBy(m => m.Title).ToListAsync());
    public IActionResult Create() => View("Edit", new Movie());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Genre,ReleaseYear,Rating,Duration,PosterUrl,Description")] Movie movie) {
        if (!ModelState.IsValid) return View("Edit", movie);
        db.Movies.Add(movie); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int id) { var movie = await db.Movies.FindAsync(id); return movie is null ? NotFound() : View(movie); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,ReleaseYear,Rating,Duration,PosterUrl,Description")] Movie movie) {
        if (id != movie.Id) return BadRequest();
        if (!ModelState.IsValid) return View(movie);
        db.Update(movie); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Delete(int id) { var movie = await db.Movies.FindAsync(id); return movie is null ? NotFound() : View(movie); }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) { var movie = await db.Movies.FindAsync(id); if (movie is not null) { db.Movies.Remove(movie); await db.SaveChangesAsync(); } return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Reviews() => View(await db.MovieReviews.Include(r => r.Movie).Include(r => r.User).OrderByDescending(r => r.CreatedAt).AsNoTracking().ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ModerateReview(int id) { var review = await db.MovieReviews.FindAsync(id); if (review is not null) { review.IsApproved = !review.IsApproved; await db.SaveChangesAsync(); } return RedirectToAction(nameof(Reviews)); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id) { var review = await db.MovieReviews.FindAsync(id); if (review is not null) { db.MovieReviews.Remove(review); await db.SaveChangesAsync(); } return RedirectToAction(nameof(Reviews)); }
    public async Task<IActionResult> Users() => View(await db.Users.OrderBy(u => u.Email).AsNoTracking().ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserLockout(string id)
    {
        if (id == User.FindFirstValue(ClaimTypes.NameIdentifier)) { TempData["Error"] = "You cannot lock your own admin account."; return RedirectToAction(nameof(Users)); }
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        var currentlyLocked = user.LockoutEnd > DateTimeOffset.UtcNow;
        await users.SetLockoutEnabledAsync(user, true);
        await users.SetLockoutEndDateAsync(user, currentlyLocked ? null : DateTimeOffset.UtcNow.AddYears(100));
        return RedirectToAction(nameof(Users));
    }
}
