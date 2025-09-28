using System.Text.Json;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InventoryManagement.Domain.Repository;

public interface IHistoryRepository
{
    Task AddAsync(History history);

    Task AddAsync<T>(T bill, string action, string user);

    Task<List<History>> GetAllAsync();

}
public class HistoryRepository : IHistoryRepository
{
    private readonly ApplicationDbContext applicationDbContext;

    public HistoryRepository(ApplicationDbContext context){
        applicationDbContext = context;
    }

    public async Task AddAsync(History history)
    {
        await applicationDbContext.Histories.AddAsync(history);
    }

    public async Task AddAsync<T>(T t, string action, string user)
    {
        string json = JsonSerializer.Serialize(t);

        History hs = new History(){
            Type = typeof(T).Name,
            User = user,
            Details = json,
            Action = action
        };
        await AddAsync(hs);
    }

    public async Task<List<History>> GetAllAsync()
    {
        return await applicationDbContext.Histories.ToListAsync();
    }
}