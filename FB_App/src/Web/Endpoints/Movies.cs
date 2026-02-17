using FB_App.Application.Common.Models;
using FB_App.Application.Movies.Commands.CreateMovie;
using FB_App.Application.Movies.Commands.DeleteMovie;
using FB_App.Application.Movies.Commands.UpdateMovie;
using FB_App.Application.Movies.Queries.GetMovieById;
using FB_App.Application.Movies.Queries.GetMovies;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FB_App.Web.Endpoints;

public sealed class Movies : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMovies, "/")
            .Produces<PaginatedList<MovieDto>>(StatusCodes.Status200OK)
            .WithDescription("Returns a paginated list of movies");

        groupBuilder.MapGet(GetMovieById, "/{id}")
            .Produces<MovieDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Returns movie details by ID");

        groupBuilder.MapPost(CreateMovie, "/")
            .RequireAuthorization()
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithDescription("Creates a new movie");

        groupBuilder.MapPut(UpdateMovie, "/{id}")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Updates an existing movie");

        groupBuilder.MapDelete(DeleteMovie, "/{id}")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Deletes a movie by ID");
    }

    public static async Task<Ok<PaginatedList<MovieDto>>> GetMovies(
        ISender sender,
        [AsParameters] GetMoviesQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<MovieDetailDto>, ProblemHttpResult>> GetMovieById(
        ISender sender,
        Guid id)
    {
        var result = await sender.Send(new GetMovieByIdQuery(id));

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(
                detail: $"Movie with ID '{id}' was not found",
                title: "Movie Not Found",
                statusCode: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }

        return TypedResults.Ok(result.Value);
    }

    public static async Task<Results<Created<Guid>, ValidationProblem>> CreateMovie(
        ISender sender,
        CreateMovieCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/movies/{id}", id.Value);
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> UpdateMovie(
        ISender sender,
        Guid id,
        UpdateMovieCommand command)
    {
        if (id != command.Id)
        {
            return TypedResults.Problem(
                detail: "The ID in the URL does not match the ID in the request body",
                title: "ID Mismatch",
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1");
        }

        var result = await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteMovie(ISender sender, Guid id)
    {
        var result = await sender.Send(new DeleteMovieCommand(id));
        return TypedResults.NoContent();
    }
}
