using FB_App.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FB_App.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<Comment> Comments { get; } = [];
}
