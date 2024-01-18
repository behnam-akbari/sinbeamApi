using Scaphoid.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .CreateLogger();

var AllowSpecificOrigins = "_allowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddApplicationService();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

var app = builder.Build();

app.UseStaticFiles();

app.UseCors(AllowSpecificOrigins);

app.UseAuthorization();

//app.Map("/main", context =>
//{
//    context.Response.StatusCode = StatusCodes.Status200OK;

//    return Task.CompletedTask;
//});

app.MapFallbackToFile("/main", @"index.html");
app.MapFallbackToFile("/", @"index.html");

app.MapControllers();

app.Run();
