namespace FB_App.Domain.Events.Movies;

public sealed class MovieDetailsUpdatedEvent(Movie movie) : BaseEvent
{
    public Movie Movie { get; } = movie;
}
