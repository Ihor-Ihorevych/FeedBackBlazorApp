using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.CreateUser;
using FB_App.Application.Users.Commands.LoginUser;
using FB_App.Application.Users.Commands.RefreshToken;
using FB_App.Application.Users.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FB_App.Web.Endpoints;

public class Users : EndpointGroupBase
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

        if (!result.IsSuccess)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { "Registration", result.Errors.Select(e => e.ToString()).ToArray() }
                },
                detail: "One or more validation errors occurred during registration.",
                title: "Registration Failed");
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>> Login(
        ISender sender,
        LoginUserCommand command)
    {
        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.ToString() ?? "Invalid credentials";
            return TypedResults.Problem(
                detail: errorMessage,
                title: "Authentication Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc7235#section-3.1");
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>> Refresh(
        ISender sender,
        RefreshTokenCommand command)
    {
        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.ToString() ?? "Invalid or expired refresh token";
            return TypedResults.Problem(
                detail: errorMessage,
                title: "Token Refresh Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc7235#section-3.1");
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<CurrentUserDto>, ProblemHttpResult>> GetCurrentUser(
        ISender sender)
    {
        var result = await sender.Send(new GetCurrentUserQuery());

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(
                detail: "User is not authenticated or session has expired",
                title: "Unauthorized",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc7235#section-3.1");
        }

        return TypedResults.Ok(result.Value);
    }
}
