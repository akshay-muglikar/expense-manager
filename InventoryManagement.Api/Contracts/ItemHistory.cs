using System;

namespace InventoryManagement.Api.Contracts;

public class ItemHistory
{
    public int Id { get; set; }
    public int? BillId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int QuantityUpdated { get; set; }
    public DateTimeOffset Date { get; set; }
    public string User { get; set; }

}
