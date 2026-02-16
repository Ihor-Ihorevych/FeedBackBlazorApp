using FB_App.Application.Comments.Commands.ApproveComment;
using FB_App.Application.Comments.Commands.CreateComment;
using FB_App.Application.Comments.Commands.RejectComment;
using FB_App.Application.Comments.Queries.GetCommentsByMovie;
using FB_App.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FB_App.Web.Endpoints;

public class Comments : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateComment, "/")
            .RequireAuthorization()
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Creates a new comment for a movie");

        groupBuilder.MapGet(GetCommentsByMovie, "/movie/{movieId}")
            .AllowAnonymous()
            .Produces<List<CommentDetailDto>>(StatusCodes.Status200OK)
            .WithDescription("Returns all comments for a specific movie");

        groupBuilder.MapPut(ApproveComment, "/movie/{movieId}/{id}/approve")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Approves a pending comment (Admin only)");

        groupBuilder.MapPut(RejectComment, "/movie/{movieId}/{id}/reject")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Rejects a pending comment (Admin only)");
    }

    public static async Task<Results<Created<Guid>, ProblemHttpResult>> CreateComment(
        ISender sender,
        CreateCommentCommand command)
    {
        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(
                detail: $"Movie with ID '{command.MovieId}' was not found",
                title: "Movie Not Found",
                statusCode: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }

        return TypedResults.Created($"/api/comments/{result.Value}", result.Value);
    }

    public static async Task<Ok<List<CommentDetailDto>>> GetCommentsByMovie(
        ISender sender,
        Guid movieId,
        CommentStatus? status = null)
    {
        var query = new GetCommentsByMovieQuery
        {
            MovieId = movieId,
            Status = status
        };

        var comments = await sender.Send(query);
        return TypedResults.Ok(comments);
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> ApproveComment(
        ISender sender,
        Guid movieId,
        Guid id)
    {
        var result = await sender.Send(new ApproveCommentCommand(movieId, id));

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(
                detail: $"Comment with ID '{id}' was not found for movie '{movieId}'",
                title: "Comment Not Found",
                statusCode: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> RejectComment(
        ISender sender,
        Guid movieId,
        Guid id)
    {
        var result = await sender.Send(new RejectCommentCommand(movieId, id));

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(
                detail: $"Comment with ID '{id}' was not found for movie '{movieId}'",
                title: "Comment Not Found",
                statusCode: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }

        return TypedResults.NoContent();
    }
}
