using System;
using InventoryManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public interface ILoginReporsitory{
    Task ContactUs(Contact contact);
    Task CreateUser(User userModel);
    Task<List<ClientModel>> GetAllClients();
    Task<ClientModel> GetClient(Guid guid);
    public Task<Model.User> GetUser(string username, string password);
    Task<List<User>> GetUsersAsync(Guid id);
}
public class LoginRepository : ILoginReporsitory
{
    private readonly IdentityDbContext _context;
    public LoginRepository(IdentityDbContext context){
        _context = context;
    }

    public Task ContactUs(Contact contact)
    {
        _context.ContactUs.Add(contact);
        return _context.SaveChangesAsync();
    }

    public Task CreateUser(User userModel)
    {
        _context.Users.Add(userModel);
        return _context.SaveChangesAsync();
    }

    public Task<List<ClientModel>> GetAllClients()
    {
        return _context.Clients.AsNoTracking().ToListAsync();
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

    public Task<List<User>> GetUsersAsync(Guid id)
    {
        return _context.Users.AsNoTracking().Where(x => x.ClientId == id).ToListAsync();
    }
}
