using System;

namespace InventoryManagement.Domain.Model;

public class ClientModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime LicenseExpiryDate { get; set; } = DateTime.Now.AddMonths(3);
}
