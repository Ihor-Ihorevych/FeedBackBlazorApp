namespace FB_App.Domain.Events.Movies;

public sealed class MovieDetailsUpdatedEvent : BaseEvent
{
    public Movie Movie { get; }
    public MovieDetailsUpdatedEvent(Movie movie)
    {
        Movie = movie;
    }
}
