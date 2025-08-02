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
                page.Margin(5);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(text =>
                {
                
                    //text.Span("Powered By : ");
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new AddressComponent("From", Model.SellerAddress));
                });
                column.Spacing(10);
                column.Item().Row(row2=>{
                     row2.RelativeItem().Text($"Invoice #{Model.InvoiceNumber}")
                        .FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                });
                column.Item().Row(row2=>{
                    row2.RelativeItem().Text(text =>
                    {
                        text.Span("Date: ").SemiBold();
                        text.Span($"{Model.IssueDate:d}");
                    });
                });
            
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new SmallAddressComponent("For", Model.CustomerAddress.CompanyName, Model.CustomerAddress.Phone));
            });

            column.Item().Element(ComposeTable);
            column.Item().PaddingRight(5).AlignRight().Text($"Payment Mode: {Model.PaymentMode}").SemiBold();

            int discount = Model.Discount;
            column.Item().PaddingRight(5).AlignRight().Text($"Discount: {discount.ToString("C", InvoiceDocument.indiaCulture)}").SemiBold();
            if (Model.Advance > 0)
            {
                column.Item().PaddingRight(5).AlignRight().Text($"Advance: {Model.Advance.ToString("C", InvoiceDocument.indiaCulture)}").SemiBold();
            }


            int totalPrice = Model.InvoiceAmount;
            column.Item().PaddingRight(5).AlignRight().Text($"Bill Total: {totalPrice.ToString("C", InvoiceDocument.indiaCulture)}").SemiBold();

           
            if (!string.IsNullOrWhiteSpace(Model.Comments))
                column.Item().PaddingTop(25).Element(ComposeComments);
        });
    }

    void ComposeTable(IContainer container)
    {
        var headerStyle = TextStyle.Default.SemiBold();

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                //columns.RelativeColumn();
                //columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                //header.Cell().Text("#");
                header.Cell().Text("Name").Style(headerStyle);
                //header.Cell().AlignCenter().Text("Rate").Style(headerStyle);
                header.Cell().AlignCenter().Text("Qty").Style(headerStyle);
                header.Cell().AlignCenter().Text("Amount").Style(headerStyle);

                header.Cell().ColumnSpan(3).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var item in Model.Items)
            {
                var index = Model.Items.IndexOf(item) + 1;

                //table.Cell().Element(CellStyle).Text($"{index}");
                table.Cell().Element(CellStyle).Text(item.Name);
                //table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Price.ToString("C", InvoiceDocument.indiaCulture)}");
                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}");
                table.Cell().Element(CellStyle).AlignCenter().Text($"{(item.Price * item.Quantity).ToString()}");

                static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
        });
    }

    void ComposeComments(IContainer container)
    {
        container.ShowEntire().Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text("Comments").FontSize(13).SemiBold();
            column.Item().Text("                 ");
        });
    }
}

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

            column.Item().Text(Address.CompanyName).FontSize(17).SemiBold().AlignCenter();
            column.Item().PaddingBottom(5).LineHorizontal(1);
            column.Item().Text($"{Address.Street}, {Address.City}, {Address.State} {Address.Phone}").AlignCenter().FontSize(12);
        });
    }
}
public class SmallAddressComponent : IComponent
{
    private string Title { get; }
     private string Company { get; }
    private string Mobile { get; }

    public SmallAddressComponent(string title, string company, string mobile)
    {
        Title = title;
        Company = company;
        Mobile =mobile;
    }

    public void Compose(IContainer container)
    {
        container.ShowEntire().Column(column =>
        {
            column.Spacing(2);

            column.Item().Text($"Name   : {Company}").SemiBold();
            column.Item().Text($"Mobile : {Mobile}");

        });
    }
}
