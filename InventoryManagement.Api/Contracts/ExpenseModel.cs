using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Contracts;

public class ExpenseModel
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public int Amount { get; set; }
    public string PaymentMode { get; set; } = "CASH";
    public string ExpenseType { get; set; } = "DEBIT";
    public int? SupplierId { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
}
