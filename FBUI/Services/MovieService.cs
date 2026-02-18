using Ardalis.Result;
using FBUI.ApiClient.Contracts;
using FBUI.Extensions;

namespace FBUI.Services;

public interface IMovieService
{
    Task<Result<Guid>> CreateMovieAsync(string title, string? description, string? director, string? genre, int? releaseYear, double? rating);
    Task<Result> UpdateMovieAsync(Guid id, string title, string? description, string? director, string? genre, int? releaseYear, string? posterUrl, double? rating);
    Task<Result> DeleteMovieAsync(Guid id);
    Task<Result<MovieDetailDto>> GetMovieByIdAsync(Guid id);
    Task<Result<PaginatedListOfMovieDto>> GetMoviesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? genreFilter = null);
}

public sealed class MovieService : IMovieService
{
    private readonly IFBApiClient _apiClient;

    public MovieService(IFBApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<Result<Guid>> CreateMovieAsync(
        string title,
        string? description,
        string? director,
        string? genre,
        int? releaseYear,
        double? rating)
    {
        try
        {
            var command = new CreateMovieCommand
            {
                Title = title,
                Description = description,
                Director = director,
                Genre = genre,
                ReleaseYear = releaseYear,
                Rating = rating
            };

            var movieId = await _apiClient.CreateMovieAsync(command);
            return Result.Success(movieId);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            return Result.Error(ex.GetProblemDetails());
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error($"Problem occurred: {ex.Message}");
        }
    }

    public async Task<Result> UpdateMovieAsync(
        Guid id,
        string title,
        string? description,
        string? director,
        string? genre,
        int? releaseYear,
        string? posterUrl,
        double? rating)
    {
        try
        {
            var command = new UpdateMovieCommand
            {
                Id = id,
                Title = title,
                Description = description,
                Director = director,
                Genre = genre,
                ReleaseYear = releaseYear,
                PosterUrl = posterUrl,
                Rating = rating
            };

            await _apiClient.UpdateMovieAsync(id, command);
            return Result.Success();
        }
        catch (ApiException<ProblemDetails> ex)
        {
            return Result.Error(ex.GetProblemDetails());
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error($"Problem occurred: {ex.Message}");
        }
    }

    public async Task<Result> DeleteMovieAsync(Guid id)
    {
        try
        {
            await _apiClient.DeleteMovieAsync(id);
            return Result.Success();
        }
        catch (ApiException<ProblemDetails> ex)
        {
            return Result.Error(ex.GetProblemDetails());
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error($"Failed to delete movie: {ex.Message}");
        }
    }

    public async Task<Result<MovieDetailDto>> GetMovieByIdAsync(Guid id)
    {
        try
        {
            var movie = await _apiClient.GetMovieByIdAsync(id);
            return Result.Success(movie);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Result.NotFound();
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error($"Failed to load movie: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedListOfMovieDto>> GetMoviesAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? genreFilter = null)
    {
        try
        {
            var movies = await _apiClient.GetMoviesAsync(pageNumber, pageSize, searchTerm, genreFilter);
            return Result.Success(movies);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            return Result.Error(ex.GetProblemDetails());
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error($"Failed to load movies: {ex.Message}");
        }
    }
}
