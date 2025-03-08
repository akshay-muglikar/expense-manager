using System;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public interface IExpenseRepository
{
    Task<Expense> GetByIdAsync(int id);
    Task<IEnumerable<Expense>> GetAllAsync();
    Task AddAsync(Expense expense, string user);
    Task UpdateAsync(Expense expense, string user);
    Task DeleteAsync(int id, string user);
    Task<List<Expense>> GetFilteredAsync(DateTime? start, DateTime? end);
}

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IHistoryRepository _historyRepository;

    public ExpenseRepository(ApplicationDbContext context, IHistoryRepository historyRepository)
    {
        _context = context;
        _historyRepository =historyRepository;
    }

    public async Task<Expense> GetByIdAsync(int id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        return await _context.Expenses.ToListAsync();
    }

    public async Task AddAsync(Expense expense, string user)
    {
        _context.Expenses.Add(expense);
        await _historyRepository.AddAsync(expense, "add", user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Expense expense, string user)
    {
        _context.Expenses.Update(expense);
        await _historyRepository.AddAsync(expense, "update", user);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string user)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense != null)
        {
            _context.Expenses.Remove(expense);
            await _historyRepository.AddAsync(expense, "update", user);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<Expense>> GetFilteredAsync(DateTime? start, DateTime? end){
        var expenses = await GetAllAsync();
        return expenses.Where(x=>x.Date.DateTime>=start.Value && x.Date.DateTime<=end.Value).ToList();
    }
}

