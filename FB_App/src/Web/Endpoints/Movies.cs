using FB_App.Application.Common.Models;
using FB_App.Application.Movies.Commands.CreateMovie;
using FB_App.Application.Movies.Commands.DeleteMovie;
using FB_App.Application.Movies.Commands.UpdateMovie;
using FB_App.Application.Movies.Queries.GetMovieById;
using FB_App.Application.Movies.Queries.GetMovies;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FB_App.Web.Endpoints;

public class Movies : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMovies, "/");
        groupBuilder.MapGet(GetMovieById, "/{id}");

        groupBuilder.MapPost(CreateMovie, "/").RequireAuthorization();
        groupBuilder.MapPut(UpdateMovie, "/{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteMovie, "/{id}").RequireAuthorization();
    }

    public static async Task<Ok<PaginatedList<MovieDto>>> GetMovies(
        ISender sender, 
        [AsParameters] GetMoviesQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<MovieDetailDto>, NotFound>> GetMovieById(
        ISender sender, 
        Guid id)
    {
        var result = await sender.Send(new GetMovieByIdQuery(id));

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result.Value);
    }

    public static async Task<Created<Guid>> CreateMovie(ISender sender, CreateMovieCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/movies/{id}", id);
    }

    public static async Task<Results<NoContent, BadRequest, NotFound>> UpdateMovie(
        ISender sender, 
        Guid id, 
        UpdateMovieCommand command)
    {
        if (id != command.Id) 
            return TypedResults.BadRequest();

        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound>> DeleteMovie(ISender sender, Guid id)
    {
        var result = await sender.Send(new DeleteMovieCommand(id));

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
