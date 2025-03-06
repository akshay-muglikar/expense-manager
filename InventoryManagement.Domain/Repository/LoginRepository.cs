using System;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Domain.Repository;

public interface ILoginReporsitory{
    public Task<bool> IsValid(string username, string password);
}
public class LoginRepository : ILoginReporsitory
{
    private readonly ApplicationDbContext _context;
    public LoginRepository(ApplicationDbContext context){
        _context = context;
    }
    public async Task<bool> IsValid(string username, string password)
    {
        return await _context.Users.AnyAsync(x=>x.Username==username && x.Password==password);
    }
}
