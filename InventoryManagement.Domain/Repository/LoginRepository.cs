using System;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public interface ILoginReporsitory{
    Task<ClientModel> GetClient(Guid guid);
    public Task<Model.User> GetUser(string username, string password);
}
public class LoginRepository : ILoginReporsitory
{
    private readonly IdentityDbContext _context;
    public LoginRepository(IdentityDbContext context){
        _context = context;
    }

    public async Task<ClientModel> GetClient(Guid guid)
    {
        Console.WriteLine($"--------------{guid}");
        var clients = _context.Clients.AsNoTracking().ToList();

        return clients.FirstOrDefault(x=> x.Id == guid);
    }

    public async Task<Model.User> GetUser(string username, string password)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == password);
    }
}
