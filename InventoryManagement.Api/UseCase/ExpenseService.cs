using System;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class ExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private string _user;

    public ExpenseService(IExpenseRepository expenseRepository, UserServiceProvider userServiceProvider)
    {
        _expenseRepository = expenseRepository;
        _user = userServiceProvider?.GetUsername();
    }

    public async Task<Expense> GetByIdAsync(int id)
    {
        return await _expenseRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync(DateTime? start, DateTime? end)
    {
        if(start==null && end==null){
            return await _expenseRepository.GetAllAsync();
        }else{
            return await _expenseRepository.GetFilteredAsync(start, end);
        }
    }

    public async Task AddAsync(Expense expense)
    {
        // Add any additional validation or processing logic if needed
        await _expenseRepository.AddAsync(expense, _user);
    }

    public async Task UpdateAsync(Expense expense)
    {
        // Update business logic if required
        await _expenseRepository.UpdateAsync(expense, _user);
    }

    public async Task DeleteAsync(int id)
    {
        await _expenseRepository.DeleteAsync(id, _user);
    }
}

