using System;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;

namespace InventoryManagement.Api.UseCase;

public class ItemService: IUseCase
{
    private readonly IItemRepository _itemRepository;
    private string _user;

    public ItemService(IItemRepository itemRepository, UserServiceProvider userServiceProvider)
    {
        _itemRepository = itemRepository;
        _user = userServiceProvider.GetUsername();
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
        await _itemRepository.AddAsync(item, _user);
    }

    public async Task UpdateAsync(Item item)
    {
        await _itemRepository.UpdateAsync(item, _user);
    }

    public async Task DeleteAsync(int id)
    {
        await _itemRepository.DeleteAsync(id, _user);
    }
}
