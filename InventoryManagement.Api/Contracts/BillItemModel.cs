using System;

namespace InventoryManagement.Api.Contracts;

public class BillItemModel
{
    public int ItemId{get;set;}
    public int Quantity {get;set;}
    public int Amount {get;set;}

}
