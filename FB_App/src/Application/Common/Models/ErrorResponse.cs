namespace FB_App.Application.Common.Models;


public record ErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null);
