using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.CreateUser;
using FB_App.Application.Users.Commands.LoginUser;
using FB_App.Application.Users.Commands.RefreshToken;
using FB_App.Application.Users.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FB_App.Web.Endpoints;

public sealed class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("/register", Register)
            .AllowAnonymous()
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .WithDescription("Registers a new user account");

        groupBuilder.MapPost("/login", Login)
            .AllowAnonymous()
            .Produces<AccessTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .WithDescription("Authenticates user and returns access token");

        groupBuilder.MapPost("/refresh", Refresh)
            .AllowAnonymous()
            .Produces<AccessTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithDescription("Refreshes access token using refresh token");

        groupBuilder.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .Produces<CurrentUserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithDescription("Returns current authenticated user information");
    }

    private static async Task<Results<Ok<string>, ValidationProblem>> Register(
        ISender sender,
        CreateUserCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult, ValidationProblem>> Login(
        ISender sender,
        LoginUserCommand command)
    {
        var result = await sender.Send(command);
        if (!result.IsSuccess)
        {
            return TypedResults.Problem(statusCode:
                StatusCodes.Status401Unauthorized, title: "Invalid credentials");
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>> Refresh(
        ISender sender,
        RefreshTokenCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<CurrentUserDto>, ProblemHttpResult>> GetCurrentUser(
        ISender sender)
    {
        var result = await sender.Send(new GetCurrentUserQuery());

        return TypedResults.Ok(result.Value);
    }
}
