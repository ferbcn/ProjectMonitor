using System.Text.Json;

namespace ProjectMonitor.Endpoints;

// CommandEndpoints.cs
public static class CommandEndpoints
{
    public static void MapCommandEndpoints(this WebApplication app)
    {
        app.MapPost("/command", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var rawCommand = await reader.ReadToEndAsync();
            var command = JsonSerializer.Deserialize<Dictionary<string, string>>(rawCommand)!["command"];
            var siteUrl = JsonSerializer.Deserialize<Dictionary<string, string>>(rawCommand)!["siteUrl"];
            var response = ProcessCommand(command);
            
            // return response to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { output = siteUrl + ": " + response }));
        });
    }

    private static string ProcessCommand(string command)
    {
        return command.ToLower() switch
        {
            "help" => "Available commands: help, about, clear, stats, load, exit",
            "stats" => $"Memory: {GC.GetTotalMemory(false) / 1024000} MBytes, CPU: {Environment.ProcessorCount} cores",
            "about" => "Terminal Simulator v1.0. Created using HTML, CSS, and JavaScript.",
            "clear" => string.Empty,
            _ => $"Command not found: {command}"
        };
    }
}