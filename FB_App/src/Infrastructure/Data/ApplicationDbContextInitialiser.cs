using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
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

public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task InitialiseAsync()
    {
        try
        {
            // See [https://jasontaylor.dev/ef-core-database-initialisation-strategies](https://jasontaylor.dev/ef-core-database-initialisation-strategies)
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
                // Original movies
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
                    8.9),

                // Additional classic movies
                Movie.Create(
                    "Forrest Gump",
                    "The history of the US from the 1950s to the '70s unfolds from the perspective of an Alabama man with an IQ of 75, who yearns to be reunited with his childhood sweetheart.",
                    1994,
                    "Robert Zemeckis",
                    "Drama",
                    null,
                    8.8),
                Movie.Create(
                    "Schindler's List",
                    "In German-occupied Poland during World War II, industrialist Oskar Schindler gradually becomes concerned for his Jewish workforce after witnessing their persecution by the Nazis.",
                    1993,
                    "Steven Spielberg",
                    "Biography",
                    null,
                    9.0),
                Movie.Create(
                    "The Lord of the Rings: The Return of the King",
                    "Gandalf and Aragorn lead the World of Men against Sauron's army to draw his gaze from Frodo and Sam as they approach Mount Doom with the One Ring.",
                    2003,
                    "Peter Jackson",
                    "Action",
                    null,
                    9.0),
                Movie.Create(
                    "Fight Club",
                    "An insomniac office worker and a devil-may-care soap maker form an underground fight club that evolves into something much more.",
                    1999,
                    "David Fincher",
                    "Drama",
                    null,
                    8.8),
                Movie.Create(
                    "The Matrix",
                    "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.",
                    1999,
                    "Lana Wachowski, Lilly Wachowski",
                    "Action",
                    null,
                    8.7),

                // Modern blockbusters
                Movie.Create(
                    "Interstellar",
                    "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.",
                    2014,
                    "Christopher Nolan",
                    "Adventure",
                    null,
                    8.7),
                Movie.Create(
                    "Parasite",
                    "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan.",
                    2019,
                    "Bong Joon Ho",
                    "Thriller",
                    null,
                    8.5),
                Movie.Create(
                    "Avengers: Infinity War",
                    "The Avengers and their allies must be willing to sacrifice all in an attempt to defeat the powerful Thanos before his blitz of devastation and ruin puts an end to the universe.",
                    2018,
                    "Anthony Russo, Joe Russo",
                    "Action",
                    null,
                    8.4),
                Movie.Create(
                    "Dune",
                    "A noble family becomes embroiled in a war for control over the galaxy's most valuable asset. Their involvement will bring the royal house much glory, or utter destruction.",
                    2021,
                    "Denis Villeneuve",
                    "Action",
                    null,
                    8.3),
                Movie.Create(
                    "Oppenheimer",
                    "The story of American scientist J. Robert Oppenheimer and his role in the development of the atomic bomb.",
                    2023,
                    "Christopher Nolan",
                    "Biography",
                    null,
                    8.4),

                // Additional variety
                Movie.Create(
                    "Goodfellas",
                    "The story of Henry Hill and his life in the mob, covering his relationship with his wife Karen Hill and his mob partners Jimmy Conway and Tommy DeVito in the Italian-American crime syndicate.",
                    1990,
                    "Martin Scorsese",
                    "Biography",
                    null,
                    8.7),
                Movie.Create(
                    "Se7en",
                    "Two detectives, a rookie and a veteran, hunt a serial killer who uses the seven deadly sins as his motives.",
                    1995,
                    "David Fincher",
                    "Crime",
                    null,
                    8.6),
                Movie.Create(
                    "Gladiator",
                    "A former Roman General sets out to exact vengeance against the corrupt emperor who murdered his family and sent him into slavery.",
                    2000,
                    "Ridley Scott",
                    "Action",
                    null,
                    8.5),
                Movie.Create(
                    "The Silence of the Lambs",
                    "A young F.B.I. cadet must receive the help of an incarcerated and manipulative cannibal killer to help catch another serial killer, a madman who skins his victims.",
                    1991,
                    "Jonathan Demme",
                    "Thriller",
                    null,
                    8.6),
                Movie.Create(
                    "Spirited Away",
                    "During her family's move to the suburbs, a sullen 10-year-old girl wanders into a world ruled by gods, witches, and spirits, and where humans are changed into beasts.",
                    2001,
                    "Hayao Miyazaki",
                    "Animation",
                    null,
                    8.6)
            };

            _context.Movies.AddRange(movies);
            await _context.SaveChangesAsync();

            var regularUser = await _userManager.FindByEmailAsync("user@localhost");
            var adminUser = await _userManager.FindByEmailAsync("administrator@localhost");

            if (regularUser != null)
            {
                // Comments from regular user
                var comment1 = movies[0].AddComment(regularUser.Id, "One of the best movies ever made! A masterpiece.");
                var comment2 = movies[0].AddComment(regularUser.Id, "The friendship between Andy and Red is so touching.");
                var comment3 = movies[1].AddComment(regularUser.Id, "A timeless classic. Marlon Brando's performance is legendary.");
                var comment4 = movies[2].AddComment(regularUser.Id, "Heath Ledger's Joker is unforgettable!");
                var comment5 = movies[3].AddComment(regularUser.Id, "Mind-bending and visually stunning.");
                var comment6 = movies[5].AddComment(regularUser.Id, "Life is like a box of chocolates, you never know what you're gonna get!");
                var comment7 = movies[6].AddComment(regularUser.Id, "One of the most powerful films about the Holocaust.");
                var comment8 = movies[11].AddComment(regularUser.Id, "The music, the visuals, the story - perfection!");


                await _context.SaveChangesAsync();

                if (adminUser != null)
                {
                    movies[0].ApproveComment(comment1.Id, adminUser.Id);
                    movies[0].ApproveComment(comment2.Id, adminUser.Id);
                    movies[1].ApproveComment(comment3.Id, adminUser.Id);
                    movies[2].ApproveComment(comment4.Id, adminUser.Id);
                    movies[3].ApproveComment(comment5.Id, adminUser.Id);
                    movies[5].ApproveComment(comment6.Id, adminUser.Id);
                    movies[6].ApproveComment(comment7.Id, adminUser.Id);
                    movies[11].ApproveComment(comment8.Id, adminUser.Id);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
