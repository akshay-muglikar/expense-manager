using System;

namespace InventoryManagement.Domain.Model;

public class Expense
{
    public int Id {get; set;}
    public string? Description {get; set;}
    public string User {get; set;}
    public int Amount {get; set;}
    public PaymentMode PaymentMode { get; set; } = PaymentMode.CASH;
    public ExpenseType ExpenseType { get; set; } = ExpenseType.DEBIT;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

}
public enum ExpenseType
{
    DEBIT,
    CREDIT
}