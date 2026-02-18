using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using FB_App.Domain.Events.Movies;

namespace FB_App.Application.Movies.Commands.DeleteMovie;

[Authorize(Roles = Roles.Administrator)]
public sealed record DeleteMovieCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteMovieCommandHandler(IApplicationDbContext context, ICacheService cache) : IRequestHandler<DeleteMovieCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICacheService _cache = cache;

    public async Task<Result> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FindAsync([(MovieId)request.Id], cancellationToken);

        if (movie == null)
        {
            return Result.NotFound($"{nameof(Movie)} ({request.Id}) was not found.");
        }

        movie.AddDomainEvent(new MovieDeletedEvent(movie));

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.MovieById(request.Id), cancellationToken);

        return Result.Success();
    }
}
