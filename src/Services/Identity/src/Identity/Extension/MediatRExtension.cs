using BuildingBlocks.Logging;
using BuildingBlocks.Validation;
using Identity.Configurations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Extension
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddCustomMediatR(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IdentityRoot).Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
