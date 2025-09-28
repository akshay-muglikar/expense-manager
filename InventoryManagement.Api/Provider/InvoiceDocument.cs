using System;
using System.Globalization;
using System.Linq;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InventoryManagement.Api.Contracts;
namespace InventoryManagement.Api.Provider;

public class InvoiceDocument : IDocument
{
    public static CultureInfo indiaCulture = new CultureInfo("en-IN");


    public InvoiceModel Model { get; }

    public InvoiceDocument(InvoiceModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.ContinuousSize(58, Unit.Millimetre);
                page.Margin(3);
                page.DefaultTextStyle(x => x.FontFamily("DotMatrix").LineHeight(1.1f).FontSize(11).FontColor(Colors.Black));

                page.Header().Element(ComposeHeader);
                page.Content().DefaultTextStyle(x => x.FontFamily("DotMatrix").LineHeight(1f).FontSize(11))
                .Element(ComposeContent);

                page.Footer().AlignLeft().Text(text =>
                {
                    text.Span("!! THANK YOU ** VISIT AGAIN !!");
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Company name and address (centered)
            column.Item().AlignCenter().Text(Model.SellerAddress.CompanyName.ToUpper())
                .SemiBold();
                
            column.Item().AlignCenter().Text($"{Model.SellerAddress.AddressDetails}")
                ;
            if(Model.SellerAddress.Phone != null)
                column.Item().AlignCenter().Text($"Ph:{Model.SellerAddress.Phone}")
                ;
                
            // Divider line with "Cash Memo" text
            column.Item().Row(row => {
                row.RelativeItem().AlignCenter().Text("------------ Tax Invoice ------------")
                    ;
            });
            
            // Bill number and date
            column.Item().Row(row => {
                row.RelativeItem().Text(text => {
                    text.Span("Date: ");
                    text.Span($"{Model.IssueDate:dd/MM/yy}");
                });
                
                row.RelativeItem().AlignRight().Text(text => {
                    text.Span("Bill No. : ");
                    text.Span($"{Model.InvoiceNumber}");
                });
            });
            
            // Cashier info
            // column.Item().Text(text => {
            //     text.Span("Cashier: ");
            //     text.Span(Model.SellerAddress.State); // Using State field as cashier name as it's not in the model
            // });
            
            // Divider line
            column.Item().AlignCenter().Text("--------------------------------------");
        });
    }

    void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            // Column headers for the table
            column.Item().Row(row => 
            {
                row.RelativeItem(2).Text("Particulars");
                row.RelativeItem((float)0.8).AlignCenter().Text("Qty");
                row.RelativeItem().AlignCenter().Text("Rate");
                row.RelativeItem((float)1.3).AlignCenter().Text("Amount");
            });
            column.Item().AlignCenter().Text("--------------------------------------");

            // Table with items
            column.Item().Element(ComposeTable);

                    
            // Subtotal
            decimal subTotal = Model.Items.Sum(x => x.Price * x.Quantity);
            column.Item().AlignRight().Text("--------------");

            column.Item().Row(row =>
                {
                    row.RelativeItem(2).AlignRight().Text("Sub Total : ");
                    row.RelativeItem().AlignRight().Text($"{subTotal:F2}");
                });

            if (Model.Discount > 0)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).AlignRight().Text("Discount : ");
                    row.RelativeItem().AlignRight().Text($"{Model.Discount:F2}");
                });
            }
            if (Model.Advance > 0)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).AlignRight().Text("Advance : ");
                    row.RelativeItem().AlignRight().Text($"{Model.Advance:F2}");
                });
            }
                column.Item().AlignCenter().Text("--------------------------------------");

            int totalRounded = (int)Math.Round(subTotal - Model.Discount - Model.Advance);
             column.Item().Row(row =>
            {
                row.RelativeItem(2).AlignRight().Text("Total : ");
                row.RelativeItem().AlignRight().Text($"{totalRounded:F2}");
            });
                column.Item().AlignCenter().Text("--------------------------------------");

                     
            // Payment mode
            column.Item().PaddingRight(5).AlignRight().Text($"Payment mode : {Model.PaymentMode}");
                column.Item().AlignCenter().Text("                                ");

            // // Bottom row with GSTIN if available
            // if (!string.IsNullOrEmpty(Model.Comments))
            // {
            //     column.Item().AlignCenter().Text($"GSTIN: {Model.Comments}");
            // }
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Column(column => 
        {
            foreach (var item in Model.Items)
            {
                column.Item().Row(row => 
                {
                    row.RelativeItem(3).Text(item.Name);
                    row.RelativeItem().AlignLeft().Text($"{item.Quantity}");
                    row.RelativeItem().AlignLeft().Text($"{item.Price}");
                    row.RelativeItem().AlignRight().Text($"{item.Price * item.Quantity}");
                });
            }
        });
    }

    // No longer needed since comments are handled in ComposeContent
}

// Not used in the new format, but kept for backward compatibility
public class AddressComponent : IComponent
{
    private Address Address { get; }

    public AddressComponent(string title, Address address)
    {
        Address = address;
    }

    public void Compose(IContainer container)
    {
        container.ShowEntire().Column(column =>
        {
            column.Spacing(2);

            column.Item().Text(Address.CompanyName).SemiBold().AlignCenter();
            column.Item().PaddingBottom(5).LineHorizontal(1);
            column.Item().Text($"{Address.AddressDetails}").AlignCenter();
        });
    }
}

// Not used in the new format, but kept for backward compatibility
public class SmallAddressComponent : IComponent
{
    private string Title { get; }
    private string Company { get; }
    private string Mobile { get; }

    public SmallAddressComponent(string title, string company, string mobile)
    {
        Title = title;
        Company = company;
        Mobile = mobile;
    }

    public void Compose(IContainer container)
    {
        container.ShowEntire().Column(column =>
        {
            column.Spacing(2);

            column.Item().Text($"Name   : {Company}");
            column.Item().Text($"Mobile : {Mobile}");
        });
    }
}
