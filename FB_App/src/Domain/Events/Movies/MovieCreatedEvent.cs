namespace FB_App.Domain.Events.Movies;

public sealed class MovieCreatedEvent(Movie movie) : BaseEvent
{
    public Movie Movie { get; } = movie;
}
