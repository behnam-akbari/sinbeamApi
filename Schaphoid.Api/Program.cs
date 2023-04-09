using Scaphoid.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplicationService();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
