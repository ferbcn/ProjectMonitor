using System.Drawing;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Channels;

namespace ProjectMonitor;

public static class Endpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        var contStreamOn = true;
        
        // Server Sent Events (SSE) endpoint
        app.MapGet("/api-stream", (Func<HttpContext, Task>)(async context =>
        {
            var channel = Channel.CreateUnbounded<string>();

            var jsonString = await File.ReadAllTextAsync("site_list.json");
            var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);
            
            var totalSites = json.Count;
            var processedSites = 0;

            if (totalSites < 1)
            {
                await context.Response.WriteAsync("Error: No sites found in site_list.json");
                return;
            }

            _ = Task.Run(async () =>
                {
                    foreach (var site in json)
                    {
                        ProcessSiteAsync(site, channel.Writer);
                    }
                });

            context.Response.ContentType = "text/event-stream";
            await foreach (var data in channel.Reader.ReadAllAsync())
            {
                await context.Response.WriteAsync($"data: {data}\n\n");
                await context.Response.Body.FlushAsync();
                processedSites++;
                if (processedSites == totalSites) {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await channel.Writer.WriteAsync("DONE");
                    await context.Response.Body.FlushAsync();
                    // channel.Writer.Complete();
                }
            }
            
            // yield control to the runtime, allow other tasks to run asynchronously
            await Task.Yield();
            
        }));
        
        async static Task ProcessSiteAsync(Site.Site site, ChannelWriter<string> writer)
        {
            Console.WriteLine("Processing Task for site: " + site.url);
            

            
            var color = new Color();
            try
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                var full_url = "https://" + site.url;
                stopwatch.Start();
                
                // ping site 
                var ping = new Ping();
                try {
                    var result = ping.Send(site.url);
                    site.up = result.Status == IPStatus.Success;
                    site.pingMillis = (int)result.RoundtripTime;
                }
                catch (PingException e) {
                    Console.WriteLine("Ping Error!");
                    site.up = false;
                    site.pingMillis = -1;
                }
                
                // download site
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(full_url);
                site.downloadSize = (int) response.Content.Headers.ContentLength;
                stopwatch.Stop();
                site.downloadMillis = stopwatch.ElapsedMilliseconds;
                site.up = true;
                if (site.downloadMillis > 150) {
                    color = Color.FromArgb(150, 250, 250, 0);
                }
                else {
                    color = Color.FromArgb(150, 100, 200, 100);
                }
                site.colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
                await writer.WriteAsync(JsonSerializer.Serialize(site));
            }
            catch (Exception e)
            {
                color = Color.FromArgb(150, 200, 50, 50);
                site.colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
                if (e is HttpRequestException) {
                    Console.WriteLine("Site Not reachable: " + site.url);
                }
                else {
                    Console.WriteLine("Other Site Error: " + e);
                }

                site.up = false;
                site.pingMillis = -1;
                await writer.WriteAsync(JsonSerializer.Serialize(site));
            }
            site.colorHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
            Console.WriteLine("Finished task for: " + site.url);
        }
    }
}