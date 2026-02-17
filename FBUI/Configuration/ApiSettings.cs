namespace FBUI.Configuration;

/// <summary>
/// Configuration settings for API connections.
/// </summary>
public sealed class ApiSettings
{
    public const string SectionName = "Api";
    public string BaseAddress { get; set; } = "https://localhost:5001";
    public string AdminNotificationsHubUrl => $"{BaseAddress}/hubs/admin-notifications";
}
