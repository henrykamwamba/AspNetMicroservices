using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Infrastucture.Email;
using Ordering.Infrastucture.Persistence;
using Ordering.Infrastucture.Repositories;

namespace Ordering.Infrastucture
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OrderingConnectionString")));

            services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.Configure<EmailSettings>(c => {
                c.FromName = configuration["EmailSettings:FromAddress"];
                c.ApiKey = configuration["EmailSettings:ApiKey"];
                c.FromAddress = configuration["EmailSettings:FromAddress"];
            });
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}