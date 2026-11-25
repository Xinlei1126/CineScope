using CineScope.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieReview> MovieReviews => Set<MovieReview>();
    public DbSet<MovieFavorite> MovieFavorites => Set<MovieFavorite>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Movie>().Property(m => m.Rating).HasPrecision(3, 1);
        builder.Entity<MovieFavorite>().HasKey(f => new { f.UserId, f.MovieId });
        builder.Entity<MovieReview>().HasIndex(r => new { r.UserId, r.MovieId }).IsUnique();
        builder.Entity<MovieReview>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
