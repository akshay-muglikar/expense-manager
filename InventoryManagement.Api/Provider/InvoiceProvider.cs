using System.Linq;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.Utility;
using InventoryManagement.Domain.Model;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace InventoryManagement.Api.Provider;

public class InvoiceProvider
{

    public static Stream GetInvoiceA5(Bill bill, List<BillItem> billitems)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        List<OrderItem> items = new List<OrderItem>();
        foreach (var item in billitems)
        {
            OrderItem oi = new OrderItem();
            oi.Name = item.Item.Name;
            oi.Price = item.Amount;
            oi.Quantity = item.Quantity;
            items.Add(oi);
        }

        var model = new InvoiceModel
        {
            InvoiceNumber = bill.Id,
            IssueDate = bill.BillDate.DateTime,
            DueDate = DateTime.Now + TimeSpan.FromDays(14),
            SellerAddress = new Address()
            {
                CompanyName = "Sai Car Decor",
                Phone = "8796148014",
                State = "Maharashtra",
                City = "Pune",
                Street = "Pune Nagar road, Ubale Nagar, Wagholi"
            },
            CustomerAddress = new Address()
            {
                CompanyName = bill.Name,
                Phone = bill.Mobile
            },
            Items = items,
            Comments = "test",
            InvoiceAmount = BillUtility.Calculate(items, bill.Discount, bill.Advance),
            Discount = bill.Discount,
            PaymentMode = bill.PaymentMode.ToString(),
            Advance = bill.Advance
        };
        var document = new InvoiceDocumentA5(model);
        var tempfile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        File.WriteAllBytes(tempfile, document.GeneratePdf());
        Console.WriteLine(tempfile);
        return File.Open(tempfile, FileMode.Open);
    }
    public static Stream GetInvoice(Bill bill, List<BillItem> billitems)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        List<OrderItem> items = new List<OrderItem>();
        foreach (var item in billitems)
        {
            OrderItem oi = new OrderItem();
            oi.Name = item.Item.Name;
            oi.Price = item.Amount;
            oi.Quantity = item.Quantity;
            items.Add(oi);
        }

        var model = new InvoiceModel
        {
            InvoiceNumber = bill.Id,
            IssueDate = bill.BillDate.DateTime,
            DueDate = DateTime.Now + TimeSpan.FromDays(14),
            SellerAddress = new Address()
            {
                CompanyName = "Sai Car Decor",
                Phone = "8796148014",
                State = "Maharashtra",
                City = "Pune",
                Street = "Pune Nagar road, Ubale Nagar, Wagholi"
            },
            CustomerAddress = new Address()
            {
                CompanyName = bill.Name,
                Phone = bill.Mobile
            },
            Items = items,
            Comments = "test",
            InvoiceAmount = BillUtility.Calculate(items, bill.Discount, bill.Advance),
            Discount = bill.Discount,
            PaymentMode = bill.PaymentMode.ToString(),
            Advance = bill.Advance
        };
        var document = new InvoiceDocumentA5(model);
        var tempfile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        File.WriteAllBytes(tempfile, document.GeneratePdf());
        Console.WriteLine(tempfile);
        return File.Open(tempfile, FileMode.Open);

    }
    
    
}
