using System;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Contracts;

public class BillModel
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string Mobile {get; set;}
    public int? Discount {get; set;}
    public int? Advance {get; set;}
    public string? PaymentMode {get; set;}
    public DateTimeOffset BillDate{get; set;} = DateTimeOffset.Now;
    public  string? Status { get; set;}


}
