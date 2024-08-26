using ControleCerto.Bus;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace ControleCerto.Extensions
{
    internal static class AppServiceExtensions
    {
        public static void AddRabbitMQService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.AddConsumer<ConsoleMessageEventConsumer>();
                busConfigurator.AddConsumer<SendEmailEventConsumer>();
                busConfigurator.AddConsumer<SendConfirmEmailEventConsumer>();
                busConfigurator.AddConsumer<SendForgotPasswordEventConsumer>();

                busConfigurator.UsingRabbitMq((ctx, cfg) =>
                {

                    
                    cfg.Host(new Uri(configuration.GetConnectionString("RabbitMQ")!), host =>
                    {
                        host.Username("user");
                        host.Password("12345");

                        cfg.ConfigureEndpoints(ctx);
                    });
                });
            });
        }

        public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetSection("Redis:Configuration").Value;
                options.InstanceName = configuration.GetSection("Redis:InstanceName").Value;
            });
        }
    }
}
