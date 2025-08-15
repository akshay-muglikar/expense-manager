using System;

namespace InventoryManagement.Domain.Model;

public class Supplier
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string? Mobile {get; set;}
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

}
