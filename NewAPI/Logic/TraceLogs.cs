using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json;

public class TraceLogs
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TraceLogs> _logger;
    private readonly IWebHostEnvironment _env;
    private const int MAX_REQUEST_MS = 300;

    public TraceLogs(RequestDelegate next, ILogger<TraceLogs> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    private class LogObject(string endpoint, int duration, string timestamp)
    {
        public string Endpoint { get; set; } = endpoint;
        public int Duration { get; set; } = duration;
        public string Timestamp { get; set; } = timestamp;
    }

    private async Task<bool> SendDiscordMessage(string endpoint, int duration)
    {
        string webhookUrl = "https://discord.com/api/webhooks/1446134654829727826/y_hPsvL4kB6mE3-eVQoKqF5qM18jtY8oevx-AA2-dpO8xbBRQ75yhWjxH3Um97PUJT6C";

        var payload = new
        {
            content = $"**Te sloom request verwerkt**\n\n" +
                      $"Request naar `{endpoint}` duurde **{duration - MAX_REQUEST_MS}**ms te lang (Totaal: **{duration}**ms)."
                      + "\n@everyone"
        };

        var json = JsonSerializer.Serialize(payload);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var client = new HttpClient();
            await client.PostAsync(webhookUrl, data);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij versturen Discord webhook. Endpoint: {Endpoint}, duration: {Duration}.", endpoint, data);

            return false;
        }
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            string endpoint = context.Request.Path.Value?.ToLower() ?? "/";
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            int duration = (int)sw.ElapsedMilliseconds;

            LogObject logObject = new(endpoint, duration, timestamp);

            string logLine = JsonSerializer.Serialize(logObject);

            string logsDirectory = Path.Combine(_env.ContentRootPath, "Logs");
            Directory.CreateDirectory(logsDirectory);

            string fileName = $"perf-{DateTime.UtcNow:dd-MM-yyyy}.log";
            string filePath = Path.Combine(logsDirectory, fileName);

            try
            {
                await File.AppendAllTextAsync(filePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LOG SCHRIJVEN NAAR {FilePath} MISLUKT", filePath);
            }

            if (duration > MAX_REQUEST_MS)
                await SendDiscordMessage(endpoint, duration);
        }
    }
}
