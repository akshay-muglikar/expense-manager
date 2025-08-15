using System;
using System.Globalization;
using System.Linq;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InventoryManagement.Api.Contracts;
namespace InventoryManagement.Api.Provider;

public class InvoiceDocumentA5 : IDocument
{
    public static CultureInfo indiaCulture = new CultureInfo("en-IN");


    public InvoiceModel Model { get; }

    public InvoiceDocumentA5(InvoiceModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(10); // Reduced margin for A5
                page.DefaultTextStyle(x => x.FontFamily("Arial").LineHeight(1.1f).FontSize(8).FontColor(Colors.Black)); // Smaller font

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Top section with company info and TAX INVOICE
            column.Item().Row(row =>
            {
                // Left side - Company details
                row.RelativeItem(2).Column(leftColumn =>
                {
                    leftColumn.Item().Text(Model.SellerAddress.CompanyName)
                        .FontSize(12).SemiBold(); // Reduced from 16
                    leftColumn.Item().Text($"{Model.SellerAddress.Street}, {Model.SellerAddress.City}, {Model.SellerAddress.State}")
                        .FontSize(8); // Reduced from 10
                    leftColumn.Item().Text($"Phone: {Model.SellerAddress.Phone}")
                        .FontSize(8);
                
                    // leftColumn.Item().Text($"GSTIN: {Model.Comments ?? "N/A"}")
                    //     .FontSize(8);
                    // leftColumn.Item().Text($"PAN Number: {Model.SellerAddress.State}") // Using State as PAN placeholder
                    //     .FontSize(8);
                });

                // Right side - Invoice details
                row.RelativeItem().Column(rightColumn =>
                {
                    rightColumn.Item().AlignRight().Text("TAX INVOICE")
                        .FontSize(10).SemiBold(); // Reduced from 14
                    rightColumn.Spacing(2); // Reduced spacing
                    rightColumn.Item().AlignRight().Text($"Invoice No: {Model.InvoiceNumber}")
                        .FontSize(8);
                    rightColumn.Item().AlignRight().Text($"Invoice Date: {Model.IssueDate:dd MMM yyyy}")
                        .FontSize(8);
                    // rightColumn.Item().AlignRight().Text($"Email Id: info@company.com")
                    //     .FontSize(8);
                    // rightColumn.Item().AlignRight().Text($"Website: www.company.com")
                    //     .FontSize(8);
                });
            });

            column.Item().PaddingTop(10).BorderBottom(2).BorderColor(Colors.Black); // Reduced padding

            // Bill To section only
            column.Item().PaddingTop(5).Row(row => // Reduced padding
            {
                // Bill To
                row.RelativeItem().Column(billToColumn =>
                {
                    billToColumn.Item().Text(Model.CustomerAddress.CompanyName).FontSize(9).SemiBold(); // Reduced from 11
                    billToColumn.Item().Text($"Phone: {Model.CustomerAddress.Phone}")
                        .FontSize(8);
                });
            });

            column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black); // Reduced padding
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Spacing(5);
            // Main table for items
            column.Item().Element(ComposeMainTable);
            if (Model.Discount > 0)
            {
                // Discount section only
                column.Item().PaddingTop(2).Row(row =>
                {
                    row.RelativeItem().Column(rightColumn =>
                    {
                        
                        rightColumn.Item().Row(summaryRow =>
                        {
                                   summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column

                            summaryRow.RelativeItem().AlignRight().Text("Discount");
                            summaryRow.RelativeItem().AlignRight().Text($"Rs. {Model.Discount:F2}");
                        });
                    });
                });
            }
            if (Model.Advance > 0)
                {
                    column.Item().PaddingTop(2).Row(row =>
                    {
                        row.RelativeItem().Column(rightColumn =>
                        {
                            rightColumn.Item().Row(summaryRow =>
                            {
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column

                                summaryRow.RelativeItem().AlignRight().Text("Advance");
                                summaryRow.RelativeItem().AlignRight().Text($"Rs. {Model.Advance:F2}");
                            });
                        });
                    });
                }

            // Total section
                column.Item().BorderTop(1).BorderColor(Colors.Black).PaddingTop(3).Row(row =>
            {
                row.RelativeItem().AlignRight(); // Empty space for rate column
                        row.RelativeItem().AlignRight(); // Empty space for rate column
                        row.RelativeItem().AlignRight(); // Empty space for rate column
                        row.RelativeItem().AlignRight(); // Empty space for rate column

                row.RelativeItem().AlignRight().Text("TOTAL").FontSize(9).SemiBold();
                row.RelativeItem().AlignRight().Text($"Rs. {Model.InvoiceAmount:F0}").FontSize(9).SemiBold();
            });
            column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(rightColumn =>
                        {
                            rightColumn.Item().Row(summaryRow =>
                            {
                                  summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column
                                summaryRow.RelativeItem().AlignRight(); // Empty space for rate column

                                summaryRow.RelativeItem().AlignRight().Text("Payment Mode: ");
                                summaryRow.RelativeItem().AlignRight().Text($"{Model.PaymentMode}");
                              
                            });
                        });
                    });
            // Tax breakdown table
            column.Item().PaddingTop(10).Element(ComposeTaxTable);
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

    void ComposeMainTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30); // S. No.
                columns.RelativeColumn(3); // Item
                columns.ConstantColumn(60); // Quantity
                columns.ConstantColumn(60); // Rate
                columns.ConstantColumn(80); // Amount
            });

            // Header
            table.Header(header =>
            {
                header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(3).Text("S.No").FontSize(7).SemiBold();
                header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(3).Text("Item").FontSize(7).SemiBold();
                header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(3).Text("Qty").FontSize(7).SemiBold();
                header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(3).Text("Rate").FontSize(7).SemiBold();
                header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(3).Text("Amount").FontSize(7).AlignRight().SemiBold();
            });

            // Data rows
            for (int i = 0; i < Model.Items.Count; i++)
            {
                var item = Model.Items[i];

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).Text($"{i + 1}").FontSize(7);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).Text(item.Name.Length > 20 ? item.Name.Substring(0, 20) + "..." : item.Name).FontSize(7);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).Text($"{item.Quantity}").FontSize(7);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).Text($"{item.Price:F0}").FontSize(7);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).AlignRight().Text($"{item.Price * item.Quantity:F0}").FontSize(7);
            }

            // Add fewer empty rows for A5
            for (int i = 0; i < 1; i++)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("");
            }
        });
    }

    void ComposeTaxTable(IContainer container)
    {
        
    }

    void ComposeFooter(IContainer container)
    {
        container.PaddingTop(20).Row(row =>
        {
            // Remark section only
            row.RelativeItem().Column(leftColumn =>
            {
                leftColumn.Item().Text("Remark : ").FontSize(10);
            });

            // Signature section only
            row.RelativeItem().Column(rightColumn =>
            {
                rightColumn.Item().PaddingTop(20).AlignRight().Text("Authorised Signature").FontSize(10);
                rightColumn.Item().AlignRight().Text(Model.SellerAddress.CompanyName).FontSize(10).SemiBold();
            });
        });
    }

    // No longer needed since comments are handled in ComposeContent
}
