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
        return exception?.Result?.Errors switch
        {
            null => exception?.Message ?? "An error occurred.",
            _ => string.Join(", ", exception.Result.Errors
            .Select(error => $"{error.Key}: {string.Join(", ", error.Value)}"))
        };
    }

    /// <summary>
    /// Retrieves a detailed error message from the specified API exception, including any additional problem details if
    /// available.
    /// </summary>
    /// <remarks>If the exception contains additional properties with error details, these are combined into a
    /// single message. If no additional details are present, the method returns the exception's message or a generic
    /// error message. This method is intended to simplify error reporting for API exceptions that include structured
    /// problem details.</remarks>
    /// <param name="exception">The API exception containing problem details from which to extract the error message. Cannot be null.</param>
    /// <returns>A string containing the concatenated error messages from the problem details if present; otherwise, the
    /// exception message or a default error message.</returns>
    public static string GetProblemDetails(this ApiException<ProblemDetails> exception)
    {
        if (exception.Result.AdditionalProperties is not { Count: > 0 } props)
        {
            return exception?.Message ?? "An error occurred.";
        }

        var message = props?.Values.Select(error => $"{string.Join(", ", error)}").Aggregate((a, b) => $"{a}{Environment.NewLine}{b}")
               ?? exception?.Message ?? "An error occurred.";

        return message;
    }
}
