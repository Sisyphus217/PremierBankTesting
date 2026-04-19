using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PremierBankTesting.Application.Abstractions.Bank;
using PremierBankTesting.Application.Abstractions.Persistence;
using PremierBankTesting.Infrastructure.Bank;
using PremierBankTesting.Infrastructure.Persistence;

namespace PremierBankTesting.Infrastructure.Extensions;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IBankApiClient, BankApiClient>();

        return services;
    }
}
