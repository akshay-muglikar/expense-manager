using System;
using InventoryManagement.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Domain;

public static class DependecyConfiguration
{
    public static IServiceCollection AddDomainDependencies(this IServiceCollection services){

        services.AddScoped<IItemRepository, ItemRepository>();

        // Register Repositories
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IBillRepository, BillRepository>();

        return services;
    }
}
