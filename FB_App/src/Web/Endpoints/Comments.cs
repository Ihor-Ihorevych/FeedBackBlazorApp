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
        groupBuilder.MapPost(CreateComment, "/").RequireAuthorization();
        groupBuilder.MapGet(GetCommentsByMovie, "/movie/{movieId}").RequireAuthorization();
        groupBuilder.MapPut(ApproveComment, "/movie/{movieId}/{id}/approve").RequireAuthorization();
        groupBuilder.MapPut(RejectComment, "/movie/{movieId}/{id}/reject").RequireAuthorization();
    }

    public static async Task<Results<Created<Guid>, NotFound>> CreateComment(ISender sender, CreateCommentCommand command)
    {
        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound();
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

    public static async Task<Results<NoContent, NotFound>> ApproveComment(ISender sender, Guid movieId, Guid id)
    {
        var result = await sender.Send(new ApproveCommentCommand(movieId, id));

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound>> RejectComment(ISender sender, Guid movieId, Guid id)
    {
        var result = await sender.Send(new RejectCommentCommand(movieId, id));

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
