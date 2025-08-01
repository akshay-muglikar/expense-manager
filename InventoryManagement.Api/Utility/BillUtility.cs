
using InventoryManagement.Api.Contracts;
using InventoryManagement.Domain.Model;

namespace InventoryManagement.Api.Utility;


public static class BillUtility
{
    public static int Calculate(List<OrderItem> items, int discount, int advance)
    {
        return items.Sum(x => x.Quantity * x.Price) - discount - advance;
    }
    
    public static int Calculate(List<BillItemModel> items, int discount, int advance)
    {
        return items.Sum(x => x.Quantity * x.Amount) - discount - advance;
    }
}