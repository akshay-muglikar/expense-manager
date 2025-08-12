using System;

namespace InventoryManagement.Domain.Model;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Car { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public string? Barcode{ get; set; }
    
}
