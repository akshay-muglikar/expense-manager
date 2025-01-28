using System;

namespace InventoryManagement.Domain.Model;

public class Expense
{
    public int Id {get; set;}
    public string? Description {get; set;}
    public int User {get; set;}
    public int Amount {get; set;}

}
