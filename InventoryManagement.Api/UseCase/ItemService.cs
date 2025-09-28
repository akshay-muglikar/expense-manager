using System;
using System.Text;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;
using InventoryManagement.Domain.Repository;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace InventoryManagement.Api.UseCase;

public class ItemService : IUseCase
{
    private readonly IItemRepository _itemRepository;
    private readonly IBillRepository _billRepository;
    private string _user;
    private IMemoryCache _cache;

    public ItemService(IItemRepository itemRepository, IBillRepository billRepository, UserServiceProvider userServiceProvider, IMemoryCache cache)
    {
        _itemRepository = itemRepository;
        _billRepository = billRepository;
        _user = userServiceProvider.GetUsername();
        _cache = cache;
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

    internal async Task<byte[]> GenerateCsvAsync()
    {
        var items = await _itemRepository.GetAllAsync();
        var csv = new StringBuilder();
        csv.AppendLine("Id,Name,Car,Quantity,Description,Barcode,Price");
        foreach (var item in items)
        {
            csv.AppendLine($"{item.Id},{item.Name},{item.Car},{item.Quantity},{item.Description},{item.Barcode},{item.Price}");
        }
        return Encoding.UTF8.GetBytes(csv.ToString());
    }
    internal async Task<(int, int)> UploadCsv(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        string line;
        List<Item> items = new List<Item>();
        await reader.ReadLineAsync(); // Skip header line
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var values = line.Split(',');
            if (values.Length < 7) continue; // Skip invalid lines
            int.TryParse(values[0], out int id);
            var item = new Item
            {
                Id = id,
                Name = values[1],
                Car = values[2],
                Quantity = int.Parse(values[3]),
                Description = values[4],
                Barcode = values[5],
                Price = int.Parse(values[6])
            };
            items.Add(item);
        }
        return await _itemRepository.AddOrUpdateAsync(items, _user);
    }

    internal async Task ScanItemAsync(string barcode, string clientId)
    {
        var item = await _itemRepository.GetByBarcodeAsync(barcode);
        if (item == null)
        {
            string key = $"{clientId}_inventory";
            _cache.Set(key, barcode, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
        else
        {
            string key = $"{clientId}_bill";
            _cache.Set(key, item.Id, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
        // Here you can add any additional logic for scanning, like logging or updating inventory
    }

    internal async Task<List<ItemHistory>> GetItemHistoryAsync()
    {
        List<ItemHistory> itemHistory = new List<ItemHistory>();
        var historyList = await _itemRepository.GetHistoryAsync();
        HashSet<(int, int)> processedItems = new HashSet<(int, int)>();
        historyList.ForEach(item =>
        {
            if (item.Type == "Item")
            {
                var itemDetails = JsonConvert.DeserializeObject<Item>(item.Details);
                itemHistory.Add(new ItemHistory
                {
                    Id = itemDetails.Id,
                    Name = itemDetails.Name,
                    QuantityUpdated = itemDetails.Quantity,
                    Type = "Inventory",
                    Date = item.Date ?? DateTime.Now,
                    User = item.User
                });
            }
        });
        var billItems = await _billRepository.GetBillItems();
        billItems.ForEach(billItem =>
        {
            
                itemHistory.Add(new ItemHistory
                {
                    Id = billItem.ItemId,
                    Name = billItem.Item.Name,
                    BillId = billItem.BillId,
                    QuantityUpdated = -billItem.Quantity,
                    Type = "Bill",
                    Date = billItem.Bill.BillDate,
                    User = billItem.Bill.User
                });
               
        });
        return itemHistory.OrderByDescending(x => x.Date).ToList();
    }
}
