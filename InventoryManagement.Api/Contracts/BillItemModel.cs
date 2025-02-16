using System;

namespace InventoryManagement.Api.Contracts;

public class BillItemModel
{
    public string Item{get;set;}
    public int ItemId  {get {
        if(int.TryParse(Item, out int id))
            return id;
        return 0;
    }}
    public string? OtherItem {get{
          if(int.TryParse(Item, out int id))
            return null;
        return Item;
    }}
    public int Quantity {get;set;}
    public string Amount {get;set;}

}
