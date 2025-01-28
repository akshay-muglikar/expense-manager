using System;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class ExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Expense> GetByIdAsync(int id)
    {
        return await _expenseRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        return await _expenseRepository.GetAllAsync();
    }

    public async Task AddAsync(Expense expense)
    {
        // Add any additional validation or processing logic if needed
        await _expenseRepository.AddAsync(expense);
    }

    public async Task UpdateAsync(Expense expense)
    {
        // Update business logic if required
        await _expenseRepository.UpdateAsync(expense);
    }

    public async Task DeleteAsync(int id)
    {
        await _expenseRepository.DeleteAsync(id);
    }
}

