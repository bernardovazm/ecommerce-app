using Ecommerce.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ProductReadRepository, ProductReadRepository>();
            return services;
        }
    }
}
