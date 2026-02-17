using FBUI.ApiClient;
using FBUI.ApiClient.Contracts;

namespace FBUI.Extensions;

public static class ApiExceptionExtensions
{
    /// <summary>
    /// Extracts and formats validation errors from ApiException.
    /// </summary>
    /// <param name="exception">The API exception containing validation errors.</param>
    /// <returns>A formatted string with all validation errors.</returns>
    public static string GetValidationErrors(this ApiException<HttpValidationProblemDetails> exception)
    {
        if (exception?.Result?.Errors == null)
        {
            return exception?.Message ?? "An error occurred.";
        }

        return string.Join(", ", exception.Result.Errors
            .Select(error => $"{error.Key}: {string.Join(", ", error.Value)}"));
    }
}
