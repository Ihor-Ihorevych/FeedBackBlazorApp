namespace FB_App.Domain.Events.Movies;

public sealed class MovieDeletedEvent(Movie movie) : BaseEvent
{
    public Movie Movie { get; } = movie;
}
