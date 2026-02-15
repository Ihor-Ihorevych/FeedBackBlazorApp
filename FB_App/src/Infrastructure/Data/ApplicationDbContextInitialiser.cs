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
                Movie.Create(
                    "The Shawshank Redemption",
                    "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                    1994,
                    "Frank Darabont",
                    "Drama",
                    null,
                    9.3),
                Movie.Create(
                    "The Godfather",
                    "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                    1972,
                    "Francis Ford Coppola",
                    "Crime",
                    null,
                    9.2),
                Movie.Create(
                    "The Dark Knight",
                    "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests.",
                    2008,
                    "Christopher Nolan",
                    "Action",
                    null,
                    9.0),
                Movie.Create(
                    "Inception",
                    "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea.",
                    2010,
                    "Christopher Nolan",
                    "Sci-Fi",
                    null,
                    8.8),
                Movie.Create(
                    "Pulp Fiction",
                    "The lives of two mob hitmen, a boxer, a gangster and his wife intertwine in four tales of violence and redemption.",
                    1994,
                    "Quentin Tarantino",
                    "Crime",
                    null,
                    8.9)
            };

            _context.Movies.AddRange(movies);
            await _context.SaveChangesAsync();

            var regularUser = await _userManager.FindByEmailAsync("user@localhost");
            
            if (regularUser != null)
            {
                var comment1 = movies[0].AddComment(regularUser.Id, "One of the best movies ever made! A masterpiece.");
                var comment2 = movies[0].AddComment(regularUser.Id, "The friendship between Andy and Red is so touching.");
                var comment3 = movies[1].AddComment(regularUser.Id, "A timeless classic. Marlon Brando's performance is legendary.");
                var comment4 = movies[2].AddComment(regularUser.Id, "Heath Ledger's Joker is unforgettable!");
                var comment5 = movies[3].AddComment(regularUser.Id, "Mind-bending and visually stunning.");

                // Save all comments first to generate IDs
                await _context.SaveChangesAsync();

                // Now approve comments with their generated IDs
                movies[0].ApproveComment(comment1.Id, administrator.Id);
                movies[0].ApproveComment(comment2.Id, administrator.Id);
                movies[1].ApproveComment(comment3.Id, administrator.Id);
                movies[2].ApproveComment(comment4.Id, administrator.Id);
                movies[3].ApproveComment(comment5.Id, administrator.Id);

                await _context.SaveChangesAsync();
            }
        }
    }
}
