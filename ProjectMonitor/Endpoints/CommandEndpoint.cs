using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

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
            var response = ProcessCommand(command, siteUrl);
            
            // return response to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { output = siteUrl + ": " + response }));
        });
    }

    private static string ProcessCommand(string command, string siteUrl)
    {
        return command.ToLower() switch
        {
            "help" => "Available commands: help, about, clear, stats, load, exit",
            "stats" => $"Memory: {GC.GetTotalMemory(false) / 1024000} MBytes, CPU: {Environment.ProcessorCount} cores",
            "about" => "Terminal Simulator v1.0. Created using HTML, CSS, and JavaScript.",
            "clear" => string.Empty,
            "load" => "Stats for site: " + GetSiteStatsAsync(siteUrl).Result,
            _ => $"Command not found: {command}"
        };
    }
    
    
    async static Task<string> GetSiteStatsAsync(string siteUrl)
    {
        var jsonString = await File.ReadAllTextAsync("site_list.json");
        var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);
        // if site is found in json.loadApi return stats
        //var site = json.FirstOrDefault(s => s.loadApi.Contains(siteUrl));
        var site = json.FirstOrDefault(s => s.url == siteUrl);        // Read MEM and CPU load from remote API if available
        if (site.loadApi != null && site.loadApi != "") {
            Console.WriteLine($"Checking stats for {site.url} ...");
            var myApiRequest = new ApiRequest();
            var okResult = await myApiRequest.MakeApiRequest(site.loadApi);
            if (okResult is OkObjectResult okObjectResult) {
                LoadStats statsObject = (LoadStats) okObjectResult.Value;
                Console.WriteLine($"API response for {site.url} - Mem: {statsObject.mem}, CPU: {statsObject.cpu}");
                site.memLoad = statsObject.mem;
                site.cpuLoad = statsObject.cpu;
            }
        }
        var stats =  $"Memory: {site.memLoad}%, CPU: {site.cpuLoad}%";
        Console.WriteLine(stats);
        return stats;
    }
}