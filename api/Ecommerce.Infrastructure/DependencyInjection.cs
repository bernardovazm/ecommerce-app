using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Auth;
using Ecommerce.Infrastructure.Email;
using Ecommerce.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default") 
                ?? "Host=localhost;Database=ecommerce;Username=postgres;Password=postgres"));
          // Repositories
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<Domain.Interfaces.IOrderRepository, OrderRepository>();
        services.AddScoped<Domain.Repositories.IOrderRepository, OrderRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPaymentRequestRepository, PaymentRequestRepository>();
        
        // Authentication Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        
        // Email Services
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Messaging Services
        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
        services.AddHostedService<PaymentRequestConsumer>();

        return services;
    }
}