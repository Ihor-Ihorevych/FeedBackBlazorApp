using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Events;

namespace FB_App.Application.Movies.Commands.CreateMovie;

[Authorize(Roles = Roles.Administrator)]
public record CreateMovieCommand : IRequest<int>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Director { get; init; }
    public string? Genre { get; init; }
    public string? PosterUrl { get; init; }
    public double? Rating { get; init; }
}

public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateMovieCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseYear = request.ReleaseYear,
            Director = request.Director,
            Genre = request.Genre,
            PosterUrl = request.PosterUrl,
            Rating = request.Rating
        };

        movie.AddDomainEvent(new MovieCreatedEvent(movie));

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return movie.Id;
    }
}
