using Microsoft.Extensions.DependencyInjection;

namespace Cricut.Orders.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainDependencies(this IServiceCollection services)
        {
            services.AddScoped<IOrderDomain, OrderDomain>();
            return services;
        }
    }
}
