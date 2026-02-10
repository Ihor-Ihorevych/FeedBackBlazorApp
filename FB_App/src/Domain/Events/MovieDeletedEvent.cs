using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

public class MovieDeletedEvent : BaseEvent
{
    public MovieDeletedEvent(Movie movie)
    {
        Movie = movie;
    }

    public Movie Movie { get; }
}
