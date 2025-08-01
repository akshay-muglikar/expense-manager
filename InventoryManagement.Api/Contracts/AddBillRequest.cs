using System;
using System.Collections.Generic;

namespace InventoryManagement.Api.Contracts;

public class AddBillRequest
{
    public string Name { get; set; }
    public string Mobile { get; set; }
    public int? Discount { get; set; }
    public int? Advance { get; set; }
    public string? PaymentMode { get; set; }
    public DateTimeOffset BillDate { get; set; } = DateTimeOffset.Now;
    public string? Status { get; set; }
    public List<BillItemModel> BillItems { get; set; }
}
