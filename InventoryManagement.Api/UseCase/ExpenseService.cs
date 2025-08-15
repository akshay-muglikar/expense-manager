using System;
using AutoMapper;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class ExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private string _user;
    private readonly IMapper _mapper;

    public ExpenseService(IExpenseRepository expenseRepository, UserServiceProvider userServiceProvider, IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _mapper = mapper;
        _user = userServiceProvider?.GetUsername() ?? "Unknown";
    }

    public async Task<ExpenseModel> GetByIdAsync(int id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        return _mapper.Map<ExpenseModel>(expense);
    }

    public async Task<IEnumerable<ExpenseModel>> GetAllAsync(DateTime? start, DateTime? end)
    {
        if(start==null && end==null){
            var expenses = await _expenseRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseModel>>(expenses);
        }else{
            var filteredExpenses = await _expenseRepository.GetFilteredAsync(start, end);
            return _mapper.Map<IEnumerable<ExpenseModel>>(filteredExpenses);
        }
    }

    public async Task AddAsync(ExpenseModel expense)
    {
        // Add any additional validation or processing logic if needed
        var expenseEntity = _mapper.Map<Expense>(expense);
        expenseEntity.User = _user; // Set the user from the service provider
        await _expenseRepository.AddAsync(expenseEntity, _user);
    }

    public async Task UpdateAsync(ExpenseModel expense)
    {
        // Update business logic if required
        var expenseEntity = _mapper.Map<Expense>(expense);
        expenseEntity.User = _user; // Set the user from the service provider

        await _expenseRepository.UpdateAsync(expenseEntity, _user);
    }

    public async Task DeleteAsync(int id)
    {
        await _expenseRepository.DeleteAsync(id, _user);
    }
}

