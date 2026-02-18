namespace FBUI.Pages.VMs;

public sealed class EditMovieViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Director { get; set; }
    public string? Genre { get; set; }
    public string? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public double? Rating { get; set; }
}
