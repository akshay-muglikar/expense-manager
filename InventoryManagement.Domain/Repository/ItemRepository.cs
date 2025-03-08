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
}

