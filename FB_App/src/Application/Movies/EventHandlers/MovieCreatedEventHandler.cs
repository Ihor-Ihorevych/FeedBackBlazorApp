using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Events.Movies;
using Microsoft.Extensions.Logging;

namespace FB_App.Application.Movies.EventHandlers;

public sealed class MovieCreatedEventHandler : INotificationHandler<MovieCreatedEvent>
{
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly ILogger<MovieCreatedEventHandler> _logger;

    public MovieCreatedEventHandler(IAdminNotificationService adminNotificationService,
                                    ILogger<MovieCreatedEventHandler> logger)
    {
        _adminNotificationService = adminNotificationService;
        _logger = logger;
    }

    public async Task Handle(MovieCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: {DomainEvent} - Movie {MovieId} created",
            notification.GetType().Name,
            notification.Movie.Id);

        await _adminNotificationService.NotifyMovieCreatedAsync(
            notification.Movie.Id,
            notification.Movie.Title,
            cancellationToken);
    }
}
