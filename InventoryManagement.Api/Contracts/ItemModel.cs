using System;

namespace InventoryManagement.Api.Contracts;

public class ItemModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Car { get; set; }
    public int Quantity { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }


}
