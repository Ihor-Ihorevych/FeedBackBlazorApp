using Ardalis.Result;
using FBUI.ApiClient.Contracts;
using FBUI.Extensions;

namespace FBUI.Services;

public interface ICommentModerationService
{
    Task<Result> ApproveCommentAsync(Guid movieId, Guid commentId);
    Task<Result> RejectCommentAsync(Guid movieId, Guid commentId);
}

public sealed class CommentModerationService(IFBApiClient apiClient) : ICommentModerationService
{
    private readonly IFBApiClient _apiClient = apiClient;

    public async Task<Result> ApproveCommentAsync(Guid movieId, Guid commentId)
    {
        try
        {
            await _apiClient.ApproveCommentAsync(movieId, commentId);
            return Result.Success();
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result> RejectCommentAsync(Guid movieId, Guid commentId)
    {
        try
        {
            await _apiClient.RejectCommentAsync(movieId, commentId);
            return Result.Success();
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result.Error(ex.GetValidationErrors());
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}