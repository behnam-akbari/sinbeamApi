using Scaphoid.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplicationService();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
