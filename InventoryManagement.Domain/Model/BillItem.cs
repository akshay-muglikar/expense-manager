using System;

namespace InventoryManagement.Domain.Model;

public class BillItem
{
    public int Id {get;set;}
    public int BillId {get;set;}
    public int ItemId  {get;set;}
    public int Quantity {get;set;}
    public string? OtherItem {get;set;}
    public int Amount {get;set;}

    public virtual Bill Bill {get;set;}
    public virtual Item? Item {get;set;}

}
