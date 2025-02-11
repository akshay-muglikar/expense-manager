using System;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Contracts;

public class BillModel
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string Mobile {get; set;}
    public int Discount {get; set;}
    public int CalculatedBillAmount {get; set;}
    public int Advance {get; set;}
    public DateTimeOffset BillDate{get; set;} = DateTimeOffset.Now;
    public  BillStatus status { get; set;} = BillStatus.NEW;
    public virtual List<BillItemModel> BillItems {get; set;} = new();


}
