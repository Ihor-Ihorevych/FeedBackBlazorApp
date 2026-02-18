using FB_App.Application.Common.Extensions;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Events.Movies;
using Microsoft.Extensions.Logging;

namespace FB_App.Application.Movies.EventHandlers;

public sealed class MovieCreatedEventHandler(IAdminNotificationService adminNotificationService,
                                ILogger<MovieCreatedEventHandler> logger) : INotificationHandler<MovieCreatedEvent>
{
    private readonly IAdminNotificationService _adminNotificationService = adminNotificationService;
    private readonly ILogger<MovieCreatedEventHandler> _logger = logger;

    public async Task Handle(MovieCreatedEvent notification, CancellationToken cancellationToken)
    {

        _logger.LogIfLevel(LogLevel.Information,
            "Domain Event: {DomainEvent} - Movie {MovieId} created",
            notification.GetType().Name,
            notification.Movie.Id);

        await _adminNotificationService.NotifyMovieCreatedAsync(
            notification.Movie.Id,
            notification.Movie.Title,
            cancellationToken);
    }
}
