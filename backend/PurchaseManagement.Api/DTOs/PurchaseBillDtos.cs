using System.ComponentModel.DataAnnotations;

namespace PurchaseManagement.Api.DTOs;

public record PurchaseBillLineInputDto(
    int? Id,
    [Required] int ItemId,
    [Required] string LocationId,
    decimal Cost,
    decimal Price,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0, 100)] decimal DiscountPercent
);

public record PurchaseBillSaveDto(
    string? SyncKey,
    [MinLength(1)] IReadOnlyList<PurchaseBillLineInputDto> Lines
);

public record PurchaseBillLineResponseDto(
    int Id,
    int ItemId,
    string ItemName,
    string LocationId,
    string LocationName,
    decimal Cost,
    decimal Price,
    int Quantity,
    decimal DiscountPercent,
    decimal LineTotalCost,
    decimal LineTotalSelling
);

public record PurchaseBillResponseDto(
    int Id,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseBillLineResponseDto> Lines,
    int TotalItems,
    int TotalQuantity,
    decimal TotalAmount,
    decimal TotalCostAmount
);

public record PurchaseBillListItemDto(int Id, DateTime CreatedAt, int LineCount, decimal TotalAmount);
