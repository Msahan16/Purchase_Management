using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Services;

public static class PurchaseBillMapper
{
    public static PurchaseBillResponseDto ToResponse(PurchaseBill bill)
    {
        var lines = bill.Items.OrderBy(i => i.Id).Select(ToLineResponse).ToList();
        var totalItems = lines.Count;
        var totalQuantity = lines.Sum(l => l.Quantity);
        var totalAmount = lines.Sum(l => l.LineTotalSelling);
        var totalCostAmount = lines.Sum(l => l.LineTotalCost);

        return new PurchaseBillResponseDto(
            bill.Id,
            bill.CreatedAt,
            lines,
            totalItems,
            totalQuantity,
            totalAmount,
            totalCostAmount
        );
    }

    public static PurchaseBillLineResponseDto ToLineResponse(PurchaseBillItem li) =>
        new(
            li.Id,
            li.ItemId,
            li.Item?.ItemName ?? string.Empty,
            li.LocationId,
            li.Location?.LocationName ?? string.Empty,
            li.Cost,
            li.Price,
            li.Quantity,
            li.Discount,
            BillMath.LineTotalCost(li.Cost, li.Quantity, li.Discount),
            BillMath.LineTotalSelling(li.Price, li.Quantity)
        );
}
