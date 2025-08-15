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
            .FirstOrDefaultAsync(b => b.Id == id) ?? throw new Exception("Bill not found");
    }

    public async Task<IEnumerable<(Bill, int TotalAmount)>> GetAllAsync()
    {
        List<(Bill, int TotalAmount)> billItems = new();
        //get all bills calculate total amount
        var bills = await _context.Bills.ToListAsync();
        foreach (var bill in bills)
        {
            var items = await _context.BillItems
                .Where(bi => bi.BillId == bill.Id)
                .ToListAsync();
            var totalAmount = items.Sum(item => item.Quantity * item.Amount);
            billItems.Add((bill, totalAmount));
        }
        return billItems;
    }
    public async Task<List<(Bill, int TotalAmount)>> GetAllAsync(DateTime? start, DateTime? end)
    {
        if (start == null || end == null)
            throw new ArgumentNullException("Start and end dates must be provided");
        var bills = (await _context.Bills.ToListAsync())
            .Where(x => x.BillDate.DateTime > start.Value && x.BillDate.DateTime < end.Value);
        List<(Bill, int TotalAmount)> billItems = new();
        foreach (var bill in bills)
        {
            var items = await _context.BillItems
                .Where(bi => bi.BillId == bill.Id)
                .ToListAsync();
            var totalAmount = items.Sum(item => item.Quantity * item.Amount);
            billItems.Add((bill, totalAmount));
        }
        return billItems;
    }

    public async Task AddAsync(Bill bill, string user)
    {
        await _context.Bills.AddAsync(bill);
        await _historyRepository.AddAsync(bill, "add", user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Bill bill, string user)
    {
        _context.Bills.Update(bill);
        await _historyRepository.AddAsync(bill, "update", user);

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

    public Task RemoveBillItem(BillItem billItem)
    {
        try
        {
            _context.Items.Where(x => x.Id == billItem.ItemId).ExecuteUpdate(x => x.SetProperty(y => y.Quantity, y => y.Quantity + billItem.Quantity));
            _context.BillItems.Remove(billItem);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Handle exception (e.g., log it)
            throw new Exception("Error removing bill item", ex);
        }
    }
    public async Task RemoveBillItems(int billId)
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var items = _context.BillItems.Where(bi => bi.BillId == billId).ToList();
            foreach (var item in items)
            {
                _context.Items.Where(x => x.Id == item.ItemId).ExecuteUpdate(x => x.SetProperty(y => y.Quantity, y => y.Quantity + item.Quantity));
            }
            _context.BillItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.CommitAsync();
        }
    }

    public async Task<List<BillItem>> getBillItems(int id)
    {
        return await _context.BillItems.Include(x => x.Item).Where(x => x.BillId == id).ToListAsync();
    }
    public async Task AddBillWithItemsAsync(Bill bill, List<BillItem> billItems, string user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Bills.AddAsync(bill);
            await _context.SaveChangesAsync();
            foreach (var item in billItems)
            {
                item.BillId = bill.Id;
                _context.Items.Where(x => x.Id == item.ItemId).ExecuteUpdate(x => x.SetProperty(y => y.Quantity, y => y.Quantity - item.Quantity));
            }
            await _context.BillItems.AddRangeAsync(billItems);
            await _historyRepository.AddAsync(bill, "add", user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<(string, string)>> GetAllCustomersAsync()
    {
        var customers = await _context.Bills
            .Select(c => new { c.Name, c.Mobile })
            .Distinct()
            .ToListAsync();
        return customers.Select(c => (c.Name, c.Mobile));
    }
    public async Task<List<(Bill, int)>> GetCustomerBillsByNameAndMobileAsync(string name, string mobile)
    {
        var billItems = new List<(Bill, int)>();
        var bills = await _context.Bills
            .Where(b => b.Name == name && b.Mobile == mobile).ToListAsync();
        foreach (var bill in bills)
        {
            var items = await _context.BillItems
                .Where(bi => bi.BillId == bill.Id)
                .ToListAsync();
            var totalAmount = items.Sum(item => item.Quantity * item.Amount);
            billItems.Add((bill, totalAmount));
        }
        return billItems;
    }
}

