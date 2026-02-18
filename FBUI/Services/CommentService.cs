using Ardalis.Result;
using FBUI.ApiClient.Contracts;
using FBUI.Extensions;

namespace FBUI.Services;

public interface ICommentService
{
    Task<Result> CreateCommentAsync(Guid movieId, string text);
    Task<Result<ICollection<CommentDetailDto>>> GetCommentsByMovieAsync(Guid movieId);
}

public sealed class CommentService(IFBApiClient apiClient) : ICommentService
{
    private readonly IFBApiClient _apiClient = apiClient;

    public async Task<Result> CreateCommentAsync(Guid movieId, string text)
    {
        try
        {
            var command = new CreateCommentCommand
            {
                MovieId = movieId,
                Text = text
            };

            await _apiClient.CreateCommentAsync(command);
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
            return Result.Error($"Failed to submit comment: {ex.Message}");
        }
    }

    public async Task<Result<ICollection<CommentDetailDto>>> GetCommentsByMovieAsync(Guid movieId)
    {
        try
        {
            var comments = await _apiClient.GetCommentsByMovieAsync(movieId, null);
            return Result.Success(comments);
        }
        catch (Exception)
        {
            return Result.Success<ICollection<CommentDetailDto>>([]);
        }
    }
}
