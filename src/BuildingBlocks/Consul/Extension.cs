using BuildingBlocks.Configuration;
using BuildingBlocks.Constants;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Consul
{
    public static class Extension
    {
        public static void AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            var consulOptions = configuration.GetOptions<ConsulOptions>(AppConstantOptions.CONSUL)
                ?? throw new ArgumentNullException("Consul options are not configured");

            services.AddSingleton(consulOptions);

            var consulClient = new ConsulClient(config =>
            {
                config.Address = consulOptions.DiscoveryAddress;
            });

            services.AddSingleton<IConsulClient, ConsulClient>(_ => consulClient);
            services.AddSingleton<IHostedService, ServiceDiscoveryRegistrationHostedService>();

        }
    }
}
