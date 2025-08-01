using System;

namespace InventoryManagement.Domain.Model;

public class Bill
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string Mobile {get; set;}
    public int Discount {get; set;}
    public int Advance { get; set; } = 0;
    public PaymentMode PaymentMode {get; set;} =  PaymentMode.UPI;
    public String PaymentUser {get; set;}
    public DateTimeOffset BillDate{get; set;} = DateTimeOffset.Now;
    public  BillStatus Status { get; set;} = BillStatus.PENDING;
    public string? User {get; set;}
}

public enum BillStatus {
    PENDING, COMPLETED, DELETED
}

public enum PaymentMode
{
    CARD,UPI,CASH
}
