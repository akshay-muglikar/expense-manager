using System;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Domain.Repository;

public interface IItemRepository
{
    Task<Item> GetByIdAsync(int id);
    Task<List<Item>> GetByIdAsync(List<int> ids);
    Task Update(Item items);
    Task<IEnumerable<Item>> GetAllAsync();
    Task AddAsync(Item item, string user);
    Task UpdateAsync(Item item, string user);
    Task DeleteAsync(int id, string user);
}
