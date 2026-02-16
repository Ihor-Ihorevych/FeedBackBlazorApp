using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Events.Movies;
using Microsoft.Extensions.Logging;

namespace FB_App.Application.Movies.EventHandlers;

public sealed class MovieDeletedEventHandler : INotificationHandler<MovieDeletedEvent>
{
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly ILogger<MovieDeletedEventHandler> _logger;

    public MovieDeletedEventHandler(IAdminNotificationService adminNotificationService,
                                    ILogger<MovieDeletedEventHandler> logger)
    {
        _adminNotificationService = adminNotificationService;
        _logger = logger;
    }

    public async Task Handle(MovieDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: {DomainEvent} - Movie {MovieId} deleted",
            notification.GetType().Name,
            notification.Movie.Id);

        await _adminNotificationService.NotifyMovieDeletedAsync(
            notification.Movie.Id,
            notification.Movie.Title,
            cancellationToken);
    }
}
