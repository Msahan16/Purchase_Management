using PurchaseManagement.Api.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PurchaseManagement.Api.Services;

public class PurchaseBillPdfService : IPurchaseBillPdfService
{
    static PurchaseBillPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(PurchaseBillResponseDto bill)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);

                page.Header().Column(col =>
                {
                    col.Item().Text("Purchase Bill").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(6).Text($"Bill # {bill.Id}").FontSize(12);
                    col.Item().Text($"Created: {bill.CreatedAt:yyyy-MM-dd HH:mm} UTC").FontSize(10).FontColor(Colors.Grey.Darken2);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2.2f);
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(0.8f);
                            c.RelativeColumn(0.7f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(1f);
                            c.RelativeColumn(1f);
                        });

                        static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(4);

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Item").SemiBold();
                            header.Cell().Element(CellStyle).Text("Batch / Loc").SemiBold();
                            header.Cell().Element(CellStyle).Text("Cost").SemiBold();
                            header.Cell().Element(CellStyle).Text("Price").SemiBold();
                            header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                            header.Cell().Element(CellStyle).Text("Disc %").SemiBold();
                            header.Cell().Element(CellStyle).Text("Tot Cost").SemiBold();
                            header.Cell().Element(CellStyle).Text("Tot Sell").SemiBold();
                        });

                        foreach (var line in bill.Lines)
                        {
                            table.Cell().Element(CellStyle).Text(line.ItemName);
                            table.Cell().Element(CellStyle).Text(line.LocationName);
                            table.Cell().Element(CellStyle).Text($"{line.Cost:F2}");
                            table.Cell().Element(CellStyle).Text($"{line.Price:F2}");
                            table.Cell().Element(CellStyle).Text(line.Quantity.ToString());
                            table.Cell().Element(CellStyle).Text($"{line.DiscountPercent:F2}");
                            table.Cell().Element(CellStyle).Text($"{line.LineTotalCost:F2}");
                            table.Cell().Element(CellStyle).Text($"{line.LineTotalSelling:F2}");
                        }
                    });

                    col.Item().PaddingTop(16).AlignRight().Column(totals =>
                    {
                        totals.Item().Text($"Total items (lines): {bill.TotalItems}").FontSize(11);
                        totals.Item().Text($"Total quantity: {bill.TotalQuantity}").FontSize(11);
                        totals.Item().Text($"Total cost amount: {bill.TotalCostAmount:F2}").FontSize(11);
                        totals.Item().Text($"Total selling amount: {bill.TotalAmount:F2}").SemiBold().FontSize(12);
                    });
                });

                page.Footer().AlignCenter().DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(t =>
                    {
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
            });
        }).GeneratePdf();
    }
}
