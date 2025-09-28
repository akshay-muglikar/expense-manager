using System;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public class ItemRepository : IItemRepository
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ApplicationDbContext _context;

    public ItemRepository(ApplicationDbContext context, IHistoryRepository historyRepository)
    {
        _context = context;
        _historyRepository = historyRepository;
    }

    public async Task<Item> GetByIdAsync(int id)
    {
        return await _context.Items.FindAsync(id);
    }
    public async Task<List<Item>> GetByIdAsync(List<int> ids)
    {
        return await _context.Items.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task Update(Item item)
    {
        _context.Update(item);
        await _context.SaveChangesAsync();
    }


    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _context.Items.ToListAsync();
    }

    public async Task AddAsync(Item item, string user)
    {
        _context.Items.Add(item);
        await _historyRepository.AddAsync(item, "add", user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Item item, string user)
    {
        _context.Items.Update(item);
        await _historyRepository.AddAsync(item, "update", user);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string user)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _historyRepository.AddAsync(item, "delete", user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(int, int)> AddOrUpdateAsync(List<Item> items, string user)
    {
        var transaction = _context.Database.BeginTransaction();
        int addedCount = 0, updatedCount = 0;
        try
        {
            foreach (var item in items)
            {
                if (item.Id == 0)
                {
                    await _context.Items.AddAsync(item);
                    await _historyRepository.AddAsync(item, "add", user);
                    addedCount++;
                }
                else
                {
                    var existingItem = _context.Items.Find(item.Id);
                    if (existingItem != null)
                    {
                        // check if any properties have changed
                        if (existingItem.Name == item.Name &&
                            existingItem.Car == item.Car &&
                            existingItem.Quantity == item.Quantity &&
                            existingItem.Description == item.Description &&
                            existingItem.Price == item.Price && existingItem.Barcode == item.Barcode)
                        {
                            continue; // No changes, skip update
                        }

                        existingItem.Name = item.Name;
                        existingItem.Car = item.Car;
                        existingItem.Quantity = item.Quantity;
                        existingItem.Description = item.Description;
                        existingItem.Price = item.Price;
                        existingItem.Barcode = item.Barcode;
                        _context.Items.Update(existingItem);
                        await _historyRepository.AddAsync(existingItem, "update", user);
                        updatedCount++;
                    }
                    else
                    {
                        await _context.Items.AddAsync(item);
                        await _historyRepository.AddAsync(item, "add", user);
                        addedCount++;
                    }
                }
            }
            await _context.SaveChangesAsync();
            transaction.Commit();
            return (addedCount, updatedCount);
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task<Item> GetByBarcodeAsync(string barcode)
    {
        return _context.Items.FirstOrDefaultAsync(x => x.Barcode == barcode);
    }
    public async Task<List<History>> GetHistoryAsync()
    {
        // Assuming HistoryRepository has a method to get history by item
        return (await _historyRepository.GetAllAsync())
            .Where(h => h.Type == "Item" || h.Type == "Bill")
            .OrderByDescending(h => h.Id)
            .ToList();
    }
}

