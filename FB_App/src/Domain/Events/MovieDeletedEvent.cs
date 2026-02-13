namespace FB_App.Domain.Events;

public sealed class MovieDeletedEvent : BaseEvent
{
    public MovieDeletedEvent(Movie movie)
    {
        Movie = movie;
    }

    public Movie Movie { get; }
}
