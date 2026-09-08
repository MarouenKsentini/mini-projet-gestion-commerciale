using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("Default") ?? "";
        services.AddDbContext<AppDbContext>(options =>
        {
            // Auto-switch: Data Source=... without Server= => SQLite (for sandbox/testing when SQL Server not installed)
            if (conn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && !conn.Contains("Server=", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(conn);
            else
                options.UseSqlServer(conn);
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddHostedService<DbSeederHostedService>();

        return services;
    }
}
