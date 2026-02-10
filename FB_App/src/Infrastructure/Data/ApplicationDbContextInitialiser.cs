using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Enums;
using FB_App.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FB_App.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);
        var userRole = new IdentityRole(Roles.User);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        if (_roleManager.Roles.All(r => r.Name != userRole.Name))
        {
            await _roleManager.CreateAsync(userRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // Default regular user
        var user = new ApplicationUser { UserName = "user@localhost", Email = "user@localhost" };

        if (_userManager.Users.All(u => u.UserName != user.UserName))
        {
            await _userManager.CreateAsync(user, "User1!");
            if (!string.IsNullOrWhiteSpace(userRole.Name))
            {
                await _userManager.AddToRolesAsync(user, new[] { userRole.Name });
            }
        }

        // Seed movies and comments
        if (!_context.Movies.Any())
        {
            var movies = new List<Movie>
            {
                new Movie
                {
                    Title = "The Shawshank Redemption",
                    Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                    ReleaseYear = 1994,
                    Director = "Frank Darabont",
                    Genre = "Drama",
                    Rating = 9.3
                },
                new Movie
                {
                    Title = "The Godfather",
                    Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                    ReleaseYear = 1972,
                    Director = "Francis Ford Coppola",
                    Genre = "Crime",
                    Rating = 9.2
                },
                new Movie
                {
                    Title = "The Dark Knight",
                    Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests.",
                    ReleaseYear = 2008,
                    Director = "Christopher Nolan",
                    Genre = "Action",
                    Rating = 9.0
                },
                new Movie
                {
                    Title = "Inception",
                    Description = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea.",
                    ReleaseYear = 2010,
                    Director = "Christopher Nolan",
                    Genre = "Sci-Fi",
                    Rating = 8.8
                },
                new Movie
                {
                    Title = "Pulp Fiction",
                    Description = "The lives of two mob hitmen, a boxer, a gangster and his wife intertwine in four tales of violence and redemption.",
                    ReleaseYear = 1994,
                    Director = "Quentin Tarantino",
                    Genre = "Crime",
                    Rating = 8.9
                }
            };

            _context.Movies.AddRange(movies);
            await _context.SaveChangesAsync();

            // Get the user ID for seeding comments
            var regularUser = await _userManager.FindByEmailAsync("user@localhost");
            
            if (regularUser != null)
            {
                var comments = new List<Comment>
                {
                    new Comment
                    {
                        MovieId = movies[0].Id,
                        UserId = regularUser.Id,
                        Text = "One of the best movies ever made! A masterpiece.",
                        Status = CommentStatus.Approved,
                        ReviewedBy = administrator.Id,
                        ReviewedAt = DateTimeOffset.UtcNow
                    },
                    new Comment
                    {
                        MovieId = movies[0].Id,
                        UserId = regularUser.Id,
                        Text = "The friendship between Andy and Red is so touching.",
                        Status = CommentStatus.Approved,
                        ReviewedBy = administrator.Id,
                        ReviewedAt = DateTimeOffset.UtcNow
                    },
                    new Comment
                    {
                        MovieId = movies[1].Id,
                        UserId = regularUser.Id,
                        Text = "A timeless classic. Marlon Brando's performance is legendary.",
                        Status = CommentStatus.Approved,
                        ReviewedBy = administrator.Id,
                        ReviewedAt = DateTimeOffset.UtcNow
                    },
                    new Comment
                    {
                        MovieId = movies[2].Id,
                        UserId = regularUser.Id,
                        Text = "Heath Ledger's Joker is unforgettable!",
                        Status = CommentStatus.Pending
                    },
                    new Comment
                    {
                        MovieId = movies[3].Id,
                        UserId = regularUser.Id,
                        Text = "Mind-bending and visually stunning.",
                        Status = CommentStatus.Approved,
                        ReviewedBy = administrator.Id,
                        ReviewedAt = DateTimeOffset.UtcNow
                    }
                };

                _context.Comments.AddRange(comments);
                await _context.SaveChangesAsync();
            }
        }
    }
}
