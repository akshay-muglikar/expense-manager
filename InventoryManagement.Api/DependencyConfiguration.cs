using System;
using InventoryManagement.Api.Provider;
using InventoryManagement.Api.UseCase;

namespace InventoryManagement.Api;

public static class DependencyConfiguration
{
    public static IServiceCollection AddApiDependencies(this IServiceCollection services){

        services.AddHttpContextAccessor();
        services.AddScoped<UserServiceProvider>();
        services.AddScoped<BillService>();
        services.AddScoped<LoginService>();
        services.AddScoped<ItemService>();
        services.AddScoped<ExpenseService>();
        services.AddScoped<SupplierService>();
        services.AddScoped<CustomerService>();
        return services;
    }
}
