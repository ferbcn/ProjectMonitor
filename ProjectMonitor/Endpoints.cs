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
            var jsonString = File.ReadAllText("site_list.json");
            var json = JsonSerializer.Deserialize<List<Site.Site>>(jsonString);

            foreach (var site in json)
            {
                var ping = new Ping();
                try
                {
                    var result = ping.Send(site.url);
                    site.up = result.Status == IPStatus.Success;
                    site.ping_time = (int)result.RoundtripTime;
                }
                catch (Exception)
                {
                    site.up = false;
                    site.ping_time = -1;
                }
            }

            foreach (var site in json)
            {
                if (!site.up) continue;
                try
                {
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    var httpRequest = (HttpWebRequest)WebRequest.Create("https://" + site.url);
                    var response = (HttpWebResponse)httpRequest.GetResponse();
                    int indexFileSize = (int)response.ContentLength;
                    stopwatch.Stop();
                    site.downloadMillis = stopwatch.ElapsedMilliseconds;
                }
                catch (SocketException)
                {
                    site.downloadMillis = -1;
                }
            }
            return Results.Ok(json);
        })
        .WithName("GetApi")
        .WithOpenApi();
        
        // Redirect from / to /dashboard
        app.MapGet("/", context =>
        {
            context.Response.Redirect("/dashboard");
            return Task.CompletedTask;
        });
        
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