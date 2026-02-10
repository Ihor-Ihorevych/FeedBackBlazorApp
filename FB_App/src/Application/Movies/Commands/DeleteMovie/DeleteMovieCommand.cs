using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Events;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Movies.Commands.DeleteMovie;

[Authorize(Roles = Roles.Administrator)]
public record DeleteMovieCommand(int Id) : IRequest;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMovieCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (movie == null)
        {
            throw new NotFoundException(nameof(Movie), request.Id.ToString());
        }

        movie.AddDomainEvent(new MovieDeletedEvent(movie));

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
