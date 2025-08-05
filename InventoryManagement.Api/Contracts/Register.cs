namespace InventoryManagement.Api.Contracts;

public class Register
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}
