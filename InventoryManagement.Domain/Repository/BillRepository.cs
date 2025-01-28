using System;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public class BillRepository : IBillRepository
{
    private readonly ApplicationDbContext _context;

    public BillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Bill> GetByIdAsync(int id)
    {
        return await _context.Bills
            .Include(b => b.BillItems)
            .ThenInclude(bi => bi.Item)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bill>> GetAllAsync()
    {
        return await _context.Bills
            .Include(b => b.BillItems)
            .ThenInclude(bi => bi.Item)
            .ToListAsync();
    }

    public async Task AddAsync(Bill bill)
    {
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Bill bill)
    {
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill != null)
        {
            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
        }
    }
}

