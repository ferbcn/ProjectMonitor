using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ProjectMonitor;

public class ApiRequest
{
    public async Task<IActionResult> MakeApiRequest(string url)
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