using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FB_App.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => CommentId.Create(value))
            .ValueGeneratedNever();

        builder.Property(c => c.MovieId)
            .HasConversion(id => id.Value, value => MovieId.Create(value))
            .IsRequired();

        builder.Property(c => c.Text)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.UserId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(CommentStatus.Pending);

        builder.Property(c => c.ReviewedBy)
            .HasMaxLength(32);

        builder.Property(c => c.ReviewedAt)
            .IsRequired(false);

        builder.HasIndex(c => c.MovieId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => new { c.MovieId, c.Status });
    }
}
