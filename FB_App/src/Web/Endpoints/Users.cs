using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.CreateUser;
using FB_App.Application.Users.Commands.LoginUser;
using FB_App.Application.Users.Commands.RefreshToken;
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
    }

    private static async Task<Results<Ok<string>, ValidationProblem>> Register(
        ISender sender,
        CreateUserCommand command)
    {
        var (result, userId) = await sender.Send(command);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { "Errors", result.Errors }
                });
        }

        return TypedResults.Ok(userId);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> Login(
        ISender sender,
        LoginUserCommand command)
    {
        var (result, token) = await sender.Send(command);

        if (!result.Succeeded || token is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(token);
    }

    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> Refresh(
        ISender sender,
        RefreshTokenCommand command)
    {
        var (result, token) = await sender.Send(command);

        if (!result.Succeeded || token is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(token);
    }
}
