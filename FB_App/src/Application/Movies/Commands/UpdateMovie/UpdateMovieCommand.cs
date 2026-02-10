using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Movies.Commands.UpdateMovie;

[Authorize(Roles = Roles.Administrator)]
public record UpdateMovieCommand : IRequest
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Director { get; init; }
    public string? Genre { get; init; }
    public string? PosterUrl { get; init; }
    public double? Rating { get; init; }
}

public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMovieCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (movie == null)
        {
            throw new NotFoundException(nameof(Movie), request.Id.ToString());
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseYear = request.ReleaseYear;
        movie.Director = request.Director;
        movie.Genre = request.Genre;
        movie.PosterUrl = request.PosterUrl;
        movie.Rating = request.Rating;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
