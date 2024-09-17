// SseEndpoints.cs

using System.Drawing;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace ProjectMonitor.Endpoints;

public static class SseEndpoints
{
    public static void MapSseEndpoints(this WebApplication app)
    {
        app.MapGet("/api/stream", async (HttpContext context) =>
        {
            var channel = Channel.CreateUnbounded<string>();
            // Read site list from json 
            var jsonString = await File.ReadAllTextAsync("site_list.json");
            var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);
            var totalSites = json.Count;
            var processedSites = 0;

            if (totalSites < 1) {
                await context.Response.WriteAsync("Error: No sites found in site_list.json");
                return;
            }

            _ = Task.Run(async () => {
                foreach (var site in json) {
                    ProcessSiteAsync(site, channel.Writer);
                }
            });
            
            // Stream data to client reading from channel
            context.Response.ContentType = "text/event-stream";
            await foreach (var data in channel.Reader.ReadAllAsync()) {
                await context.Response.WriteAsync($"data: {data}\n\n");
                await context.Response.Body.FlushAsync();
                processedSites++;
                // send complete message
                if (processedSites == totalSites){
                    await channel.Writer.WriteAsync("DONE");
                    await context.Response.Body.FlushAsync();
                }
            }
        });
    }
    
    private async static Task ProcessSiteAsync(Site.Site site, ChannelWriter<string> writer) {
        Console.WriteLine("Processing Task for site: " + site.url);
        await Task.Yield();
        
        // Read MEM and CPU load from remote API if available
        if (site.loadApi != null && site.loadApi != "") {
            var okResult = await ApiRequest(site.loadApi);
            if (okResult is OkObjectResult okObjectResult) {
                LoadStats statsObject = (LoadStats) okObjectResult.Value;
                Console.WriteLine($"API response for {site.url} - Mem: {statsObject.mem}, CPU: {statsObject.cpu}");
                site.memLoad = statsObject.mem;
                site.cpuLoad = statsObject.cpu;
            }
        }
        // ping site 
        try {
            var ping = new Ping();
            var result = ping.Send(site.url);
            site.up = result.Status == IPStatus.Success;
            site.pingMillis = (int)result.RoundtripTime;
        }
        catch (PingException) {
            Console.WriteLine("Ping Error!");
            site.up = false;
            site.pingMillis = -1;
        }
        // download site
        try {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var fullUrl = "https://" + site.url;
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(fullUrl);
            site.downloadSize = (int)response.Content.Headers.ContentLength;
            stopwatch.Stop();
            site.downloadMillis = stopwatch.ElapsedMilliseconds;
            site.up = true;
            var color = site.downloadMillis > 150 ? Color.FromArgb(150, 250, 250, 0) : 
                Color.FromArgb(150, 100, 200, 100);
            site.colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }
        catch (Exception e) {
            Console.WriteLine(e is HttpRequestException ? "Site Not reachable: " + site.url : "Other Site Error: " + e);
            site.up = false;
            site.downloadMillis = -1;
            var color = Color.FromArgb(150, 200, 50, 50);
            site.colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }
        
        // Write to channel
        await writer.WriteAsync(JsonSerializer.Serialize(site));
        Console.WriteLine("Finished task for: " + site.url);
    }
    
    public async static Task<IActionResult> ApiRequest(string url)
    {
        Console.WriteLine($"Making API request for {url} ...");
        var token = Environment.GetEnvironmentVariable("SERVER_TOKEN");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ProjectMonitor");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonResponseString = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<LoadStats>(jsonResponseString);
        return new OkObjectResult(jsonResponse);
    }
}