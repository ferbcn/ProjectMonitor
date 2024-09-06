using System.Drawing;
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

            // foreach (var site in json)
            // {
            //     var ping = new Ping();
            //     try
            //     {
            //         var result = ping.Send(site.url);
            //         site.up = result.Status == IPStatus.Success;
            //         site.ping_time = (int)result.RoundtripTime;
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine("Ping Error: " + e);
            //         // site.up = false;
            //         site.ping_time = -1;
            //     }
            // }

            foreach (var site in json)
            {
                try {
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    var httpRequest = (HttpWebRequest)WebRequest.Create("https://" + site.url);
                    var response = (HttpWebResponse)httpRequest.GetResponse();
                    site.downloadSize = (int)response.ContentLength;
                    stopwatch.Stop();
                    site.downloadMillis = stopwatch.ElapsedMilliseconds;
                    site.up = true;
                    if (site.downloadMillis > 150) { 
                        site.color = Color.FromArgb(250,220,170, 50);
                    }
                    else {
                        site.color = Color.FromArgb(150,100,200, 100);
                    }
                }
                catch (Exception e)
                {
                    if (e is WebException) {
                        Console.WriteLine("Site Unknown!");
                    }
                    else {
                        Console.WriteLine("Other Site Error: " + e);
                    }
                    site.up = false;
                    site.ping_time = -1;
                    site.color = Color.FromArgb(150,200,100, 100);
                }
            }
            return Results.Ok(json);
        })
        .WithName("GetApi")
        .WithOpenApi();
        
    }
}