using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;

namespace FB_App.Application.Movies.Commands.UpdateMovie;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateMovieCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Director { get; init; }
    public string? Genre { get; init; }
    public string? PosterUrl { get; init; }
    public double? Rating { get; init; }
}

public class UpdateMovieCommandHandler(IApplicationDbContext context, ICacheService cache) : IRequestHandler<UpdateMovieCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICacheService _cache = cache;

    public async Task<Result> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FindAsync([(MovieId)request.Id], cancellationToken);

        if (movie == null)
        {
            return Result.NotFound($"{nameof(Movie)} ({request.Id}) was not found.");
        }

        movie.UpdateDetails(
            request.Title,
            request.Description,
            request.ReleaseYear,
            request.Director,
            request.Genre,
            request.PosterUrl,
            request.Rating);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.MovieById(request.Id), cancellationToken);

        return Result.Success();
    }
}
