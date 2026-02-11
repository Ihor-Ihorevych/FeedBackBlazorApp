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
        // Public endpoints - no authorization required (Guest access)
        groupBuilder.MapGet(GetMovies, "/");
        groupBuilder.MapGet(GetMovieById, "/{id}");
        
        // Admin-only endpoints - CRUD operations
        groupBuilder.MapPost(CreateMovie, "/").RequireAuthorization();
        groupBuilder.MapPut(UpdateMovie, "/{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteMovie, "/{id}").RequireAuthorization();
    }

    public async Task<Ok<PaginatedList<MovieDto>>> GetMovies(
        ISender sender, 
        [AsParameters] GetMoviesQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<MovieDetailDto>, NotFound>> GetMovieById(
        ISender sender, 
        Guid id)
    {
        try
        {
            var movie = await sender.Send(new GetMovieByIdQuery(id));
            return TypedResults.Ok(movie);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Created<Guid>> CreateMovie(ISender sender, CreateMovieCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/movies/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest, NotFound>> UpdateMovie(
        ISender sender, 
        Guid id, 
        UpdateMovieCommand command)
    {
        if (id != command.Id) 
            return TypedResults.BadRequest();

        try
        {
            await sender.Send(command);
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Results<NoContent, NotFound>> DeleteMovie(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteMovieCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
