using System;

namespace InventoryManagement.Domain.Model;

public class User
{
    public int Id {get; set;}
    public string Username {get; set;}
    public string Password {get; set;}
    public Guid ClientId {get; set;}
}
