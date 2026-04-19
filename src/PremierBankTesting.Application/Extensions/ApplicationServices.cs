using Microsoft.Extensions.DependencyInjection;

namespace PremierBankTesting.Application.Extensions;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServices).Assembly));

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
