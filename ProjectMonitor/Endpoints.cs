using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;

namespace ProjectMonitor;

public static class Endpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        // Basic data api endpoint (JSON)
        app.MapGet("/dashboard-data", () =>
            {
                // read json file
                var jsonString = File.ReadAllText("site_list.json");
                var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);

                // ping every site and collect results in the json dictionary adding entries: up, ping_time
                foreach (var site in json) {
                    Console.WriteLine(site.ToString());
                    // ping site
                    var ping = new Ping();
                    try
                    {
                        var result = ping.Send(site.url);
                        site.up = result.Status == IPStatus.Success;
                        site.ping_time = (int)result.RoundtripTime;
                        if (site.up)
                        {
                            Console.WriteLine($"Ping to {site.url} took {site.ping_time} ms");
                        }
                        else
                        {
                            Console.WriteLine($"Ping failed for {site.url}");
                        }
                    }
                    catch (Exception ex)
                    {
                        site.up = false;
                        site.ping_time = -1;
                        Console.WriteLine($"Ping failed for {site.url}: {ex.Message}");
                    }
                }
                // download every sites landing page  
                foreach (var site in json) {
                    if (!site.up) {
                        Console.WriteLine("Site DOWN, not downloading data!");
                        continue;
                    }
                    // download site fully
                    try
                    {
                        var stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();
                        var httpRequest = (HttpWebRequest)WebRequest.Create("https://" + site.url);
                        httpRequest.Timeout = 3000; // Set timeout to 5 seconds
                        var response = (HttpWebResponse)httpRequest.GetResponse();
                        int indexFileSize = (int) response.ContentLength;
                        stopwatch.Stop();
                        Console.Write($"Full download of site {site.url} took {stopwatch.ElapsedMilliseconds} ms ");
                        Console.WriteLine($"for {indexFileSize} bytes.");
                        site.downloadMillis = stopwatch.ElapsedMilliseconds;
                    }
                    catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
                    {
                        site.downloadMillis = -1;
                        Console.WriteLine($"Full download of site {site.url} timed out.");
                    }
                    catch (SocketException ex)
                    {
                        site.downloadMillis = -1;
                        Console.WriteLine($"Full download of site failed for {site.url}: {ex.Message}");
                    }
                }
                // return json
                return Results.Ok(json);
            })
            .WithName("GetApi")
            .WithOpenApi();
        
        
    // HTML page endpoint
    app.MapGet("/dashboard", async context =>
        {
            var htmlContent = await File.ReadAllTextAsync("static/dashboard.html");
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(htmlContent);
        })
        .WithName("GetDashboardPage")
        .WithOpenApi();
    
    // JavaScript file endpoint
    app.MapGet("/dashboard.js", async context =>
        {
            var jsContent = await File.ReadAllTextAsync("static/dashboard.js");
            context.Response.ContentType = "application/javascript";
            await context.Response.WriteAsync(jsContent);
        })
        .WithName("GetDashboardJs")
        .WithOpenApi();
    
    // CSS file endpoint
    app.MapGet("/styles.css", async context =>
        {
            var cssContent = await File.ReadAllTextAsync("static/styles.css");
            context.Response.ContentType = "text/css";
            await context.Response.WriteAsync(cssContent);
        })
        .WithName("GetStylesCss")
        .WithOpenApi();
    
    
    }
}