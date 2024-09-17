using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ProjectMonitor;

// ApiRequestEndpoints.cs
public static class ApiRequestEndpoints
{
    public static void MapApiRequestEndpoints(this WebApplication app)
    {
        app.MapGet("/api/load", async (HttpContext context) =>
        {
            var okResult = await MyRequest();
            if (okResult is OkObjectResult okObjectResult)
            {
                var jsonResponse = okObjectResult.Value as string;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { output = "API Error" }));
            }
        });
    }

    private async static Task<IActionResult> MyRequest()
    {
        Console.WriteLine("Making API request...");

        var token = Environment.GetEnvironmentVariable("SERVER_TOKEN");
        var url = "https://load.mydigital.quest/api-stats";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ProjectMonitor");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{jsonResponse}\n");
        return new OkObjectResult(jsonResponse);
    }
}