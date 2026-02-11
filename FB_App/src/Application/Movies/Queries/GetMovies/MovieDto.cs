namespace FB_App.Application.Movies.Queries.GetMovies;

public class MovieDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Director { get; init; }
    public string? Genre { get; init; }
    public string? PosterUrl { get; init; }
    public double? Rating { get; init; }
    public int ApprovedCommentsCount { get; init; }
}
