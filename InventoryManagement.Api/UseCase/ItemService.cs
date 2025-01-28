using System;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class ItemService: IUseCase
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<Item> GetByIdAsync(int id)
    {
        return await _itemRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _itemRepository.GetAllAsync();
    }

    public async Task AddAsync(Item item)
    {
        await _itemRepository.AddAsync(item);
    }

    public async Task UpdateAsync(Item item)
    {
        await _itemRepository.UpdateAsync(item);
    }

    public async Task DeleteAsync(int id)
    {
        await _itemRepository.DeleteAsync(id);
    }
}
