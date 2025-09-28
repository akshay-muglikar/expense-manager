namespace InventoryManagement.Domain.Model;

public class ClientDetails
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string GSTNumber { get; set; }
    public string Address { get; set; }
    public byte[]? Logo { get; set; }
    public string InvoiceType { get; set; }
    public DateTime RegistrationDate { get; set; }
    public virtual ClientModel Client { get; set; }
}
