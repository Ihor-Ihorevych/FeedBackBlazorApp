using FB_App.Domain.Common;

namespace FB_App.Domain.Entities;

public class Movie : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int? ReleaseYear { get; set; }
    
    public string? Director { get; set; }
    
    public string? Genre { get; set; }
    
    public string? PosterUrl { get; set; }
    
    public double? Rating { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
