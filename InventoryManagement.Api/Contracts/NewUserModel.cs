using System;

namespace InventoryManagement.Api.Contracts;

public class NewUserModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    public Guid ClientId { get; set; }

}
