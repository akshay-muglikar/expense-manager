using System;

namespace InventoryManagement.Domain.Model;

public class Bill
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string Mobile {get; set;}
    public int Discount {get; set;}
    public int CalculatedBillAmount {get; set;}
    public int Advance {get; set;}
    public DateTimeOffset BillDate{get; set;} = DateTimeOffset.Now;
    public  BillStatus status { get; set;} = BillStatus.NEW;
    public virtual List<BillItem> BillItems {get; set;} = new();
    public string? User {get; set;}
}

public enum BillStatus {
    NEW,PAID,DRAFT,PENDING,DELETED
}
