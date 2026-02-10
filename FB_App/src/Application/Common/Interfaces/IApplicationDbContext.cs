using FB_App.Domain.Entities;

namespace FB_App.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Movie> Movies { get; }

    DbSet<Comment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
