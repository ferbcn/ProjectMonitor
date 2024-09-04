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
        app.MapGet("/dashboard", () =>
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
                        Console.WriteLine(result.ToString());
                    }
                    catch (Exception ex)
                    {
                        site.up = false;
                        site.ping_time = -1;
                        Console.WriteLine($"Ping failed for {site.url}: {ex.Message}");
                    }
                }
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
                        var response = (HttpWebResponse)httpRequest.GetResponse();
                        stopwatch.Stop();
                        Console.WriteLine($"Full download of site {site.url} took {stopwatch.ElapsedMilliseconds} ms");
                        site.full_donwload_millis = stopwatch.ElapsedMilliseconds;
                    }
                    catch (SocketException ex)
                    {
                        site.full_donwload_millis = -1;
                        Console.WriteLine($"Full download of site failed for {site.url}: {ex.Message}");
                    }
                }
                // return json
                return Results.Ok(json);
            })
            .WithName("GetApi")
            .WithOpenApi();
    }
}