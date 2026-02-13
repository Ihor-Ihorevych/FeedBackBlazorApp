using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using FB_App.Domain.Events;

namespace FB_App.Application.Movies.Commands.DeleteMovie;

[Authorize(Roles = Roles.Administrator)]
public record DeleteMovieCommand(Guid Id) : IRequest<Result>;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public DeleteMovieCommandHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

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
