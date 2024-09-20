using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ProjectMonitor.Endpoints;

// ApiRequestEndpoints.cs
public static class ApiRequestEndpoints
{
    public static void MapApiRequestEndpoints(this WebApplication app)
    {
        app.MapGet("/api/load", async (HttpContext context) =>
        {
            var myApiRequest = new ApiRequest();
            var okResult = await myApiRequest.MakeApiRequest("https://load.mydigital.quest/api-stats");
            if (okResult is OkObjectResult okObjectResult) {
                var jsonResponse = okObjectResult.Value as string;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);
            }
            else {
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { output = "API Error" }));
            }
        });
    }

}