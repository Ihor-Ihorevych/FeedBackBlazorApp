using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Events;

namespace FB_App.Application.Movies.Commands.DeleteMovie;

[Authorize(Roles = Roles.Administrator)]
public record DeleteMovieCommand(Guid Id) : IRequest<Result>;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteMovieCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (movie == null)
        {
            return Result.NotFound($"{nameof(Movie)} ({request.Id}) was not found.");
        }

        movie.AddDomainEvent(new MovieDeletedEvent(movie));

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
