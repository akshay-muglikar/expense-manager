namespace InventoryManagement.Api.Contracts;

public class CustomerModel
{
    public string Name { get; set; }
    public string Mobile { get; set; }

    public List<BillModel> Bills { get; set; } = new List<BillModel>();
}