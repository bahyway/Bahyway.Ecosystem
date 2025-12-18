using System.Reflection;

namespace AlarmInsight.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }
}