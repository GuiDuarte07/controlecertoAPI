using Finantech.Bus;
using MassTransit;

namespace Finantech.Extensions
{
    internal static class AppServiceExtensions
    {
        public static void AddRabbitMQService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.AddConsumer<ConsoleMessageEventConsumer>();

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
    }
}
