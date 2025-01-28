using System;
using InventoryManagement.Api.UseCase;

namespace InventoryManagement.Api;

public static class DependencyConfiguration
{
    public static IServiceCollection AddApiDependencies(this IServiceCollection services){

        services.AddScoped<BillService>();
        services.AddScoped<ItemService>();
        services.AddScoped<ExpenseService>();
        return services;
    }
}
