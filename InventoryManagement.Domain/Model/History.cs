using System;

namespace InventoryManagement.Domain.Model;

public class History
{
    public int Id { get; set;}
    public string Type { get; set;}

    public string Details { get; set;}

    public string? User {get; set;}
}
