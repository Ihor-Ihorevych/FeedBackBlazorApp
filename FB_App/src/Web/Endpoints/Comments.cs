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
        // User endpoint - create comments
        groupBuilder.MapPost(CreateComment, "/").RequireAuthorization();
        
        // Admin endpoints - moderation
        groupBuilder.MapGet(GetCommentsByMovie, "/movie/{movieId}").RequireAuthorization();
        groupBuilder.MapPut(ApproveComment, "/{id}/approve").RequireAuthorization();
        groupBuilder.MapPut(RejectComment, "/{id}/reject").RequireAuthorization();
    }

    public async Task<Created<int>> CreateComment(ISender sender, CreateCommentCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/comments/{id}", id);
    }

    public async Task<Ok<List<CommentDetailDto>>> GetCommentsByMovie(
        ISender sender, 
        int movieId,
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

    public async Task<Results<NoContent, NotFound>> ApproveComment(ISender sender, int id)
    {
        try
        {
            await sender.Send(new ApproveCommentCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Results<NoContent, NotFound>> RejectComment(ISender sender, int id)
    {
        try
        {
            await sender.Send(new RejectCommentCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
