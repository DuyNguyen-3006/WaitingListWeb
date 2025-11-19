using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using WaitingListWeb.Infrastructure.Data;
using WaitingListWeb.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load configuration (env vars override appsettings.json)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application & infra stuff
builder.Services.AddInfrastructureServices(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres"), name: "postgres", tags: new[] { "ready" });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

// Optional: diagnostic to list loaded OpenApi/Swashbuckle assemblies (debug only)
try
{
    var diagLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AssemblyDiag");
    foreach (var a in AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.GetName().Name))
    {
        var n = a.GetName().Name ?? "";
        if (n.StartsWith("Microsoft.OpenApi") || n.StartsWith("Swashbuckle"))
        {
            diagLogger.LogInformation("Loaded assembly: {Name} Version={Version} Location={Location}", n, a.GetName().Version, a.Location);
        }
    }
}
catch
{
    // ignore diagnostics errors
}

// SAFE migration with retry (configurable)
using (var scope = app.Services.CreateScope())
{
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    bool runMigrations = cfg.GetValue<bool>("AppSettings:RunMigrationsOnStartup", false);
    int maxRetries = cfg.GetValue<int>("AppSettings:MigrationsMaxRetries", 10);
    int delaySeconds = cfg.GetValue<int>("AppSettings:MigrationsRetryDelaySeconds", 2);

    if (runMigrations)
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<WaitingListDbContext>();
            var attempts = 0;
            while (true)
            {
                try
                {
                    db.Database.Migrate();
                    logger.LogInformation("Database migration applied successfully.");
                    break;
                }
                catch (Exception ex)
                {
                    attempts++;
                    logger.LogWarning(ex, "Database migration attempt {Attempt}/{Max} failed.", attempts, maxRetries);
                    if (attempts >= maxRetries)
                    {
                        logger.LogError(ex, "Max migration attempts reached. Giving up.");
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
        catch (Exception ex)
        {
            var logger2 = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger2.LogError(ex, "Critical exception while attempting migrations.");
        }
    }
}

// Middleware pipeline
// Enable Swagger (you can restrict to Development if desired)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WaitingListWeb API V1");
});

// Redirect root to swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// Exception handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("DefaultCors");
app.UseAuthorization();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = reg => reg.Tags.Contains("ready"),
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
        };
        await ctx.Response.WriteAsJsonAsync(result);
    }
});

app.MapControllers();
app.Map("/error", (HttpContext http) => Results.Problem("An unexpected error occurred."));

app.Run();
