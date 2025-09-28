using System;

namespace InventoryManagement.Api.Contracts;

public class ClientDetailsModel
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string GSTNumber { get; set; }
    public string InvoiceType { get; set; }

}
