using FB_App.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace FB_App.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Movie> Movies { get; }

    DbSet<Comment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
