using BuildingBlocks.Constants;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;

namespace BuildingBlocks.Yarp
{
    public static class Extension
    {
        public static IServiceCollection AddYarpWithConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddReverseProxy()
                .LoadFromConfig(configuration.GetSection(AppConstantOptions.RESERVE_YARP))
                .DiscoverFromConsul(configuration);

            return services;
        }

        private static IReverseProxyBuilder DiscoverFromConsul(this IReverseProxyBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;

            services.AddOptions<YarpServiceDiscoveryOptions>()
                .BindConfiguration(AppConstantOptions.YARP_SERVICE_DISCOVERY)
                .ValidateOnStart();

            var serviceProvider = services.BuildServiceProvider();


            var proxyConfigProviders = serviceProvider.GetRequiredService<IEnumerable<IProxyConfigProvider>>();

            var consulClient = serviceProvider.GetRequiredService<IConsulClient>();

            var options = serviceProvider.GetRequiredService<IOptionsMonitor<YarpServiceDiscoveryOptions>>();

            services.RemoveAll<IProxyConfigProvider>();
            services.AddSingleton<IProxyConfigProvider, YarpProxyConfigProvider>(_ =>
                new YarpProxyConfigProvider(proxyConfigProviders, consulClient, options)
            );

            return builder;
        }
    }
}
