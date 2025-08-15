using System;
using System.Collections.Generic;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Contracts;

public class GetBillModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Mobile { get; set; }
    public int? Discount { get; set; }
    public int? Advance { get; set; }
    public string? PaymentMode { get; set; }
    public DateTimeOffset BillDate { get; set; } = DateTimeOffset.Now;
    public string? Status { get; set; }
    public List<BillItemModel> BillItems { get; set; }
}

public class GetAllBillModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Mobile { get; set; }
    public int Discount { get; set; }
    public int Advance { get; set; } = 0;
    public PaymentMode PaymentMode { get; set; } = PaymentMode.UPI;
    public String PaymentUser { get; set; }
    public DateTimeOffset BillDate { get; set; } = DateTimeOffset.Now;
    public BillStatus Status { get; set; } = BillStatus.PENDING;
    public string? User { get; set; }
    public int TotalAmount { get; set; }
}
