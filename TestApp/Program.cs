// See https://aka.ms/new-console-template for more information
using InventoryManagement.Api.Provider;
using InventoryManagement.Domain.Model;

Console.WriteLine("Hello, World!");

var bill = new Bill
{
    Id = 1,
    Name = "John Doe",
    Mobile = "1234567890",
    BillDate = DateTimeOffset.Now,
    Discount = 10,
    Advance = 50,
    PaymentMode = PaymentMode.CASH,
};  
var billItems = new List<BillItem>
{
    new BillItem { Item = new Item { Name = "Item1" }, Amount = 100, Quantity = 2 },
    new BillItem { Item = new Item { Name = "Item2" }, Amount = 200, Quantity = 1 }
};
using var stream = InvoiceProvider.GetInvoiceA5(bill, billItems);
// Save or use the stream as needed
// For example, save to a file
var filePath = "/Users/akshay/InventoryApi/TestApp/invoice.pdf";
using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
stream.CopyTo(fileStream);
Console.WriteLine("Invoice generated successfully.");