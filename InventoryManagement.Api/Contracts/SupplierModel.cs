using System;

namespace InventoryManagement.Api.Contracts;

public class SupplierModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Mobile { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}