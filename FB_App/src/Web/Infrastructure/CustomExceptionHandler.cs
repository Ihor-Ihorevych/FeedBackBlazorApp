using System.Collections.Frozen;
using FB_App.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Web.Infrastructure;

/// <summary>
/// Represents a method that handles exceptions that occur during HTTP request processing.
/// </summary>
/// <remarks>Implement this delegate to customize how exceptions are handled in the HTTP pipeline, such as logging
/// errors or generating custom error responses. The delegate should not throw exceptions; any unhandled exceptions may
/// result in an incomplete response.</remarks>
/// <param name="httpContext">The HTTP context for the current request. Provides access to request and response information. Cannot be null.</param>
/// <param name="exception">The exception that was thrown during request processing. Cannot be null.</param>
/// <returns>A task that represents the asynchronous exception handling operation.</returns>
public delegate Task ExceptionHandlerDelegate(HttpContext httpContext, Exception exception);
public sealed class CustomExceptionHandler : IExceptionHandler
{
    private readonly FrozenDictionary<Type, ExceptionHandlerDelegate> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        _exceptionHandlers = new Dictionary<Type, ExceptionHandlerDelegate>()
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
                { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            }.ToFrozenDictionary();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();
        if (_exceptionHandlers.TryGetValue(exceptionType, out var value))
        {
            await value.Invoke(httpContext, exception);
            return true;
        }

        return false;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        });
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        });
    }

    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        var exception = (UnauthorizedAccessException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Detail = exception.Message
        });
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        var exception = (ForbiddenAccessException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Detail = exception.Message
        });
    }
}
