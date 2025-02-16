using System;
using System.Threading.Tasks;
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
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bill>> GetAllAsync()
    {
        return await _context.Bills
            .ToListAsync();
    }
    public async Task<List<Bill>> GetAllAsync(DateTime? start, DateTime? end)
    {
        return (await _context.Bills.ToListAsync()).Where(
            x=>x.BillDate!=null && x.BillDate.DateTime>start.Value && x.BillDate.DateTime<end.Value
        ).ToList();
    }

    public async Task AddAsync(Bill bill)
    {
        await _context.Bills.AddAsync(bill);
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

    public async Task AddBillItems(List<BillItem> billItems)
    {
        await _context.BillItems.AddRangeAsync(billItems);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveBillItems(int billId)
    {
        var items = _context.BillItems.Where(bi => bi.BillId == billId).ToList();
        _context.BillItems.RemoveRange(items);
            
    }

    public async Task<List<BillItem>> getBillItems(int id)
    {
        return await _context.BillItems.Include(x=>x.Item).Where(x=>x.BillId ==id).ToListAsync();
    }
}

