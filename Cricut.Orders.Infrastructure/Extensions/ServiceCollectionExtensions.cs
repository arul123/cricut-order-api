using Cricut.Orders.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Cricut.Orders.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IOrderStore, OrderStore>();
            return services;
        }
    }
}
