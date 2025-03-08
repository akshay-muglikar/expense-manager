using System;
using System.Threading.Tasks;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public class BillRepository : IBillRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IHistoryRepository _historyRepository;

    public BillRepository(ApplicationDbContext context, IHistoryRepository historyRepository)
    {
        _context = context;
        _historyRepository = historyRepository;
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

    public async Task AddAsync(Bill bill, string user)
    {
        await _context.Bills.AddAsync(bill);
        await _historyRepository.AddAsync(bill, "add",user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Bill bill, string user)
    {
        _context.Bills.Update(bill);
        await _historyRepository.AddAsync(bill,"update" ,user);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string user)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill != null)
        {
            _context.Bills.Remove(bill);
            await _historyRepository.AddAsync(bill, "delete", user);
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

