using CineScope.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Data;
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // EnsureCreated keeps a fresh checkout runnable. Once an initial migration is added, use migrations.
        if (db.Database.GetMigrations().Any()) await db.Database.MigrateAsync();
        else await db.Database.EnsureCreatedAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Member", "Admin" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        var email = config["CineScope:AdminEmail"];
        var password = config["CineScope:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "CineScope Admin" };
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded) throw new InvalidOperationException("Could not create configured admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        if (!await users.IsInRoleAsync(admin, "Admin")) await users.AddToRoleAsync(admin, "Admin");
        if (!await db.Movies.AnyAsync())
        {
            db.Movies.AddRange(
                new Movie { Title = "The Grand Budapest Hotel", Genre = "Comedy", ReleaseYear = 2014, Rating = 8.1m, Duration = 99, PosterUrl = "https://image.tmdb.org/t/p/w500/eWdyYQreja6JGCzqHWXpWHDrrPo.jpg", Description = "A legendary concierge and his young protégé become caught up in a theft and a battle for a priceless painting." },
                new Movie { Title = "Arrival", Genre = "Sci-Fi", ReleaseYear = 2016, Rating = 7.9m, Duration = 116, PosterUrl = "https://image.tmdb.org/t/p/w500/x2FJsf1ElLo5TYS5dkS5wqRbseX.jpg", Description = "A linguist works to communicate with visitors from beyond the stars." },
                new Movie { Title = "The Dark Knight", Genre = "Action", ReleaseYear = 2008, Rating = 9.0m, Duration = 152, PosterUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg", Description = "Batman faces a criminal mastermind who pushes Gotham into chaos." },
                new Movie { Title = "Spirited Away", Genre = "Animation", ReleaseYear = 2001, Rating = 8.6m, Duration = 125, PosterUrl = "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg", Description = "A young girl enters a mysterious spirit world and finds the courage to save her family." });
            await db.SaveChangesAsync();
        }
    }
}
