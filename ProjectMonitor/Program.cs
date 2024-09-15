using ProjectMonitor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

// Serve static files from the "wwwroot" directory
app.UseDefaultFiles();
app.UseStaticFiles();

Endpoints.MapEndpoints(app);

app.Run();
