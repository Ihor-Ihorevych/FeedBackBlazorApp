using FB_App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FB_App.Infrastructure.Data.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        // Key configuration
        builder.HasKey(m => m.Id);

        // Property configurations
        builder.Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.Director)
            .HasMaxLength(100);

        builder.Property(m => m.Genre)
            .HasMaxLength(50);

        builder.Property(m => m.PosterUrl)
            .HasMaxLength(500);

        builder.HasMany(m => m.Comments)
            .WithOne(c => c.Movie)
            .HasForeignKey(c => c.MovieId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(m => m.Title);
        builder.HasIndex(m => m.Genre);
    }
}
