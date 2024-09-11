using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Channels;

namespace ProjectMonitor;

public static class Endpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        // Basic data api endpoint (JSON)
        app.MapGet("/dashboard-data", () =>
            {
                var jsonString = File.ReadAllText("site_list.json");
                var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);

                foreach (var site in json)
                {
                    try
                    {
                        var stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();
                        var httpRequest = (HttpWebRequest)WebRequest.Create("https://" + site.url);
                        var response = (HttpWebResponse)httpRequest.GetResponse();
                        site.downloadSize = (int)response.ContentLength;
                        stopwatch.Stop();
                        site.downloadMillis = stopwatch.ElapsedMilliseconds;
                        site.up = true;
                        if (site.downloadMillis > 150)
                        {
                            site.color = Color.FromArgb(200, 220, 170, 50);
                        }
                        else
                        {
                            site.color = Color.FromArgb(150, 100, 200, 100);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is WebException)
                        {
                            Console.WriteLine("Site Not reachable:" + site.url);
                        }
                        else
                        {
                            Console.WriteLine("Other Site Error: " + e);
                        }

                        site.up = false;
                        site.ping_time = -1;
                        site.color = Color.FromArgb(150, 200, 100, 100);
                    }
                }

                return Results.Ok(json);
            })
            .WithName("GetApi")
            .WithOpenApi();

        // Server Sent Events (SSE) endpoint
        app.MapGet("/api-stream", (Func<HttpContext, Task>)(async context =>
            {
                var channel = Channel.CreateUnbounded<string>();
                _ = WriteToChannel(channel.Writer);

                context.Response.ContentType = "text/event-stream";
                // context.Response.ContentType = "application/json";
                await foreach (var data in channel.Reader.ReadAllAsync())
                {
                    await context.Response.WriteAsync($"{data}\n");
                    await context.Response.Body.FlushAsync();
                }
                
                async Task WriteToChannel(ChannelWriter<string> writer)
                {
                    await foreach (var data in GenerateEvents())
                    {
                        while (!writer.TryWrite(data))
                        {
                            // await Task.Delay(TimeSpan.FromMilliseconds(10));
                        }
                    }

                    writer.Complete();
                }
            }))
            .WithName("GetApiStream")
            .WithOpenApi();
    }

    private static async IAsyncEnumerable<string> GenerateEvents()
    {
        var jsonString = File.ReadAllText("site_list.json");
        var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);

        foreach (var site in json)
        {
            try
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var httpRequest = (HttpWebRequest)WebRequest.Create("https://" + site.url);
                var response = (HttpWebResponse)httpRequest.GetResponse();
                site.downloadSize = (int)response.ContentLength;
                stopwatch.Stop();
                site.downloadMillis = stopwatch.ElapsedMilliseconds;
                site.up = true;
                if (site.downloadMillis > 150) {
                    site.color = Color.FromArgb(200, 220, 170, 50);
                }
                else {
                    site.color = Color.FromArgb(150, 100, 200, 100);
                }
            }
            catch (Exception e)
            {
                if (e is WebException) {
                    Console.WriteLine("Site Not reachable:" + site.url);
                }
                else {
                    Console.WriteLine("Other Site Error: " + e);
                }

                site.up = false;
                site.ping_time = -1;
                site.color = Color.FromArgb(150, 200, 100, 100);
            }
            Console.WriteLine(site);
            
            yield return JsonSerializer.Serialize(site);
            await Task.Delay(TimeSpan.FromMilliseconds(site.downloadMillis/100));
        }
        
        
    }
    
}