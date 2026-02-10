using FB_App.Domain.Common;
using FB_App.Domain.Enums;

namespace FB_App.Domain.Entities;

public class Comment : BaseAuditableEntity
{
    public int MovieId { get; set; }
    
    public Movie Movie { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    
    public string Text { get; set; } = string.Empty;
    
    public CommentStatus Status { get; set; } = CommentStatus.Pending;
    
    public string? ReviewedBy { get; set; }
    
    public DateTimeOffset? ReviewedAt { get; set; }
}
