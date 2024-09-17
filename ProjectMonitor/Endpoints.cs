// Endpoints.cs

using ProjectMonitor;

public static class Endpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapCommandEndpoints();
        app.MapApiRequestEndpoints();
        app.MapSseEndpoints();
    }
}