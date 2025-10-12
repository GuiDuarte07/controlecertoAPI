using ControleCerto.Bus;
using Hangfire;
using Hangfire.PostgreSql;
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
                    });
                    
                    cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));

                    cfg.ConfigureEndpoints(ctx);
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

        public static void AddHangFireService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config =>
                config.
                    SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(c =>
                        c.UseNpgsqlConnection(configuration.GetConnectionString("WebApiDatabase"))));
        }
    }
}
