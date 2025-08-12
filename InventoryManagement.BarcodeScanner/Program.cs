using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace InventoryManagement.BarcodeScanner
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Example usage
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
            InventoryManagement.Api.Provider.InvoiceProvider invoiceProvider = new InventoryManagement.Api.Provider.InvoiceProvider();
            using var stream = invoiceProvider.GetInvoice(bill, billItems);
            // Save or use the stream as needed
            // For example, save to a file
            using var fileStream = new FileStream("invoice.pdf", FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);  
            Console.WriteLine("Invoice generated successfully.");
        }   
    }

}