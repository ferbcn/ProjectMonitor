using System.Drawing;
using System.Net;
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

                if (json != null)
                {
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
                }
                return Results.Ok(json);
            })
            .WithName("GetApi")
            .WithOpenApi();

        // Server Sent Events (SSE) endpoint
        app.MapGet("/api-stream", (Func<HttpContext, Task>)(async context =>
        {
            var channel = Channel.CreateUnbounded<string>();

            var jsonString = await File.ReadAllTextAsync("site_list.json");
            var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);

            if (json != null)
                foreach (var site in json)
                {
                    _ = ProcessSiteAsync(site, channel.Writer);
                }

            context.Response.ContentType = "text/event-stream";
            await foreach (var data in channel.Reader.ReadAllAsync())
            {
                await context.Response.WriteAsync($"data: {data}\n\n");
                await context.Response.Body.FlushAsync();
            }
        }));

        async static Task ProcessSiteAsync(Site.Site site, ChannelWriter<string> writer)
        {
            Console.WriteLine("Processing Task for site: " + site.url);
            
            // delay for debugging purposes
            // await Task.Delay(TimeSpan.FromMilliseconds(1));
            
            // yield control to the runtime, allow other tasks to run asynchronously
            await Task.Yield();
            
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
                await writer.WriteAsync(JsonSerializer.Serialize(site));
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
                await writer.WriteAsync(JsonSerializer.Serialize(site));
            }
            Console.WriteLine("Finished task for: " + site.url);
        }
    }
}