using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;

namespace FB_App.Application.Movies.Commands.CreateMovie;

[Authorize(Roles = Roles.Administrator)]
public sealed record CreateMovieCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Director { get; init; }
    public string? Genre { get; init; }
    public string? PosterUrl { get; init; }
    public double? Rating { get; init; }
}

public sealed class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateMovieCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = Movie.Create(request.Title, request.Description,
            request.ReleaseYear,
            request.Director,
            request.Genre,
            request.PosterUrl,
            request.Rating);

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);
        return (Guid)movie.Id;
    }
}
