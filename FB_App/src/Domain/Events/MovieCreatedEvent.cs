namespace FB_App.Domain.Events;

public sealed class MovieCreatedEvent : BaseEvent
{
    public MovieCreatedEvent(Movie movie)
    {
        Movie = movie;
    }

    public Movie Movie { get; }
}
