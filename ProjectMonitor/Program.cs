using ProjectMonitor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");


var app = builder.Build();

// // Add CORS services.
// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(builder => {
//         // builder.AllowAnyOrigin()
//         builder.WithOrigins(
//                 "http://localhost:8081", 
//                 "http://127.0.0.1:8081",
//                 "http://192.168.100.147:8081",
//                 "https://lively-water-0ec0ab51e.5.azurestaticapps.net"
//             )
//             .AllowAnyMethod()
//             .AllowAnyHeader();
//     });
// });


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS middleware.
// app.UseCors();

Endpoints.MapEndpoints(app);

app.Run();
