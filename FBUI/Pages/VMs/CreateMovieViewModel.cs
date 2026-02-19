namespace FBUI.Pages.VMs
{
    public sealed class CreateMovieViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Rating { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public string? Genre { get; set; }
        public string? ReleaseYear { get; set; }
    }
}
