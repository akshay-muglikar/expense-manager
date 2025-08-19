using InventoryManagement.Api.AI.Services;
using InventoryManagement.Api.AI.Services.Skills;
namespace InventoryManagement.Api.AI.DependencyConfig
{
    public static class DependencyConfig
    {
        public static IServiceCollection AddAIDependencies(this IServiceCollection services)
        {
            // Register AI helper service
            services.AddScoped<IAIHelperService, AIHelperService>();
            services.AddScoped<QueryClassifier>();
            services.AddScoped<ISkill, InventorySkill>();
            services.AddScoped<ISkill, BillingSkill>();
            services.AddScoped<ISkill, TrendsSkill>();
            services.AddScoped<ISkill, SqlAnalysisSkill>();
            services.AddScoped<ISkillRouter, SkillRouter>();
            
            return services;
        }
    }
}