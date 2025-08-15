using System;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public interface ISupplierRepository
{
    Task<Supplier> GetByIdAsync(int id);
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task AddAsync(Supplier supplier, string user);
    Task UpdateAsync(Supplier supplier, string user);
    Task<IEnumerable<Expense>> GetExpensesBySupplierIdAsync(int id);
}
public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _context;

    public SupplierRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier> GetByIdAsync(int id)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id) ?? throw new Exception("Supplier not found");
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _context.Suppliers.ToListAsync();
    }

    public async Task AddAsync(Supplier supplier, string user)
    {
        supplier.Date = DateTimeOffset.Now; // Set the date to now
        await _context.Suppliers.AddAsync(supplier);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supplier supplier, string user)
    {
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Expense>> GetExpensesBySupplierIdAsync(int id)
    {
        return await _context.Expenses
            .Where(e => e.SupplierId == id)
            .ToListAsync();
    }
}