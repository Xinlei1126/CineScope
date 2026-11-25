using System.Reflection;
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
        var migrations = db.Database.GetMigrations().ToArray();
        if (migrations.Length > 0)
        {
            // Older CineScope runs may have created the complete schema with EnsureCreated().
            // If that full schema exists but has no migration record, baseline InitialCreate
            // before MigrateAsync so EF does not try to create existing tables again.
            await BaselineLegacyEnsureCreatedDatabaseAsync(db, migrations[0]);
            await db.Database.MigrateAsync();
        }
        else
        {
            // Supports a first run before the developer has generated the InitialCreate migration.
            await db.Database.EnsureCreatedAsync();
        }
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Member", "Admin" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        var email = config["CineScope:AdminEmail"];
        var password = config["CineScope:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "CineScope Admin" };
                var result = await users.CreateAsync(admin, password);
                if (!result.Succeeded) throw new InvalidOperationException("Could not create configured admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            if (!await users.IsInRoleAsync(admin, "Admin")) await users.AddToRoleAsync(admin, "Admin");
        }

        // Demo catalog seeding is independent of optional admin account configuration.
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

    private static async Task BaselineLegacyEnsureCreatedDatabaseAsync(ApplicationDbContext db, string initialMigration)
    {
        var productVersion = typeof(DbContext).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "10.0.0";

        // Only baseline when every table from the original CineScope model exists and
        // the history table has no applied migrations. This preserves existing records.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.Movies', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.MovieReviews', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.MovieFavorites', N'U') IS NOT NULL
            BEGIN
                IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory])
                BEGIN
                    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ({initialMigration}, {productVersion});
                END;
            END;
            """);
    }
}
