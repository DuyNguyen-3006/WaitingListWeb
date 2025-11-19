using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.SharedLibrary.DependencyInjection;
using WaitingListWeb.Application.Interface;
using WaitingListWeb.Domain.Abstraction;
using WaitingListWeb.Infrastructure.Data;
using WaitingListWeb.Infrastructure.Implementation;

namespace WaitingListWeb.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            SharedServiceContainer.AddSharedServices<WaitingListDbContext>(services, config, config["MySerilog:Filename"]);

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IWaitingListService, WaitingListService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
