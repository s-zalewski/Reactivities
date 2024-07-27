using Microsoft.EntityFrameworkCore;
using Persistence;
using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();



// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

//var logger = services.GetRequiredService<ILogger<Program>>();

// logger.LogInformation("*******CONFIG START******");
// foreach (var config in builder.Configuration.AsEnumerable())
// {
//     if(config.Key.Contains("Connection") || config.Key.Contains("Microsoft.AspNetCore"))
//         logger.LogInformation($"{config.Key}: {config.Value}");
// }
// logger.LogInformation("*******CONFIG END******");

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
