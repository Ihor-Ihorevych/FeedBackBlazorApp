using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

public class MovieCreatedEvent : BaseEvent
{
    public MovieCreatedEvent(Movie movie)
    {
        Movie = movie;
    }

    public Movie Movie { get; }
}
