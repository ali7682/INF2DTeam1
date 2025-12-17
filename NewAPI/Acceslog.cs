using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

public class AccessLogs
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AccessLogs> _logger;
    private readonly IWebHostEnvironment _env;

    public AccessLogs(
        RequestDelegate next,
        ILogger<AccessLogs> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    private class LogObject(string endpoint, int userId, string role, string timestamp)
    {
        public string Endpoint { get; set; } = endpoint;
        public int User { get; set; } = userId;
        public string Role { get; set; } = role;
        public string Timestamp { get; set; } = timestamp;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        string sessionToken = context.Request.Headers.Authorization;

        string endpoint = context.Request.Path.Value.ToLower() ?? "/";
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

        int userId = user != null ? user.Id : 0;
        string? userRole = user != null ? user.Role : "none";

        LogObject logObject = new(endpoint, userId, userRole, timestamp);

        string logLine = JsonSerializer.Serialize(logObject);

        string logsDirectory = Path.Combine(_env.ContentRootPath, "Logs");
        Directory.CreateDirectory(logsDirectory);

        string fileName = $"access-{DateTime.UtcNow:dd-MM-yyyy}.log";
        string filePath = Path.Combine(logsDirectory, fileName);

        try
        {
            await File.AppendAllTextAsync(filePath, logLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LOG SCHRIJVEN NAAR {FilePath} MISLUKT", filePath);
        }
    }
}
