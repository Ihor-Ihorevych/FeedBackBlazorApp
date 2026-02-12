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
            .AllowAnonymous();

        groupBuilder.MapPost("/login", Login)
            .AllowAnonymous();

        groupBuilder.MapPost("/refresh", Refresh)
            .AllowAnonymous();

        groupBuilder.MapGet("/me", GetCurrentUser)
            .RequireAuthorization();
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
                    { "Errors", result.Errors.Select(e => e.ToString()).ToArray() }
                });
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> Login(
        ISender sender,
        LoginUserCommand command)
    {
        var result = await sender.Send(command);
        if (!result.IsSuccess)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> Refresh(
        ISender sender,
        RefreshTokenCommand command)
    {
        var result = await sender.Send(command);

        if (!result.IsSuccess)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<CurrentUserDto>, UnauthorizedHttpResult>> GetCurrentUser(
        ISender sender)
    {
        var result = await sender.Send(new GetCurrentUserQuery());

        if (!result.IsSuccess)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result.Value);
    }
}
