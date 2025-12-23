using BahyWay.RulesEngine.Core;
using BahyWay.RulesEngine.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using BahyWay.RulesEngine.Core.Interfaces; // <--- MISSING
using BahyWay.RulesEngine.Core.Core;       // <--- MISSING

namespace BahyWay.RulesEngine.Core.Extensions
{
    /// <summary>
    /// Extension methods for configuring fuzzy engine services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds fuzzy engine services to the service collection
        /// </summary>
        public static IServiceCollection AddFuzzyEngine(this IServiceCollection services)
        {
            services.AddSingleton<IFuzzyEngine, FuzzyEngine>();
            services.AddSingleton<DefuzzificationEngine>();

            return services;
        }

        /// <summary>
        /// Adds fuzzy engine services with configuration action
        /// </summary>
        public static IServiceCollection AddFuzzyEngine(
            this IServiceCollection services,
            Action<IFuzzyEngine> configure)
        {
            services.AddSingleton<IFuzzyEngine>(sp =>
            {
                var engine = ActivatorUtilities.CreateInstance<FuzzyEngine>(sp);
                configure(engine);
                return engine;
            });

            services.AddSingleton<DefuzzificationEngine>();

            return services;
        }
    }
}