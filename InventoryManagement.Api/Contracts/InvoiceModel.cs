using System;
using System.Collections.Generic;

namespace InventoryManagement.Api.Contracts;

public class InvoiceModel
{
    public int InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }

    public Address SellerAddress { get; set; }
    public Address CustomerAddress { get; set; }

    public List<OrderItem> Items { get; set; }
    public string Comments { get; set; }
    public int InvoiceAmount { get; set; }
    public int Discount { get; set; }
    public string? PaymentMode { get; set; }
    public int Advance { get; set; }

}

public class OrderItem
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
}

public class Address
{
    public string CompanyName { get; set; }
    public string AddressDetails { get; set; }
    public string Phone { get; set; }
    public byte[]? Logo { get; set; }
}

