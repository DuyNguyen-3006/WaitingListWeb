using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Project.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>
            (this IServiceCollection services, IConfiguration config, string fileName) where TContext : DbContext
        {
            //Db Context
            services.AddDbContext<TContext>(option => option.UseNpgsql(
                config
                .GetConnectionString("Postgres"), posgestserverOption =>
                posgestserverOption.EnableRetryOnFailure()));
            //config serilog 
            var logFolder = Path.Combine(AppContext.BaseDirectory, "Logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(logFolder, $"{fileName}-.txt"), // đặt vào folder Logs
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.ff zzz } [{Level:u3}] {message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();


            // Add JWT authentication
            //JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);

            //modify response 
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });
            return services;
        }

        //public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        //{
        //    //Use Global Exception
        //    app.UseMiddleware<GlobalExceptionMiddleware>();

        //    // Register middleware to block all outsiders API calls
        //    //app.UseMiddleware<ListenToOnlyApiGateWay>();

        //    return app;
        //}
    }
}
