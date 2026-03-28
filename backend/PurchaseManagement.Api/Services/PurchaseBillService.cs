using System.Text.Json;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Entities;
using PurchaseManagement.Api.Repositories;

namespace PurchaseManagement.Api.Services;

public class PurchaseBillService(
    IPurchaseBillRepository bills,
    IItemRepository items,
    ILocationRepository locations,
    IAuditLogRepository audit) : IPurchaseBillService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<PurchaseBillResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var bill = await bills.GetByIdWithLinesAsync(id, cancellationToken);
        return bill == null ? null : PurchaseBillMapper.ToResponse(bill);
    }

    public async Task<IReadOnlyList<PurchaseBillListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var headers = await bills.GetAllHeadersAsync(cancellationToken);
        return headers.Select(b =>
        {
            var totalAmount = b.Items.Sum(i => BillMath.LineTotalSelling(i.Price, i.Quantity));
            return new PurchaseBillListItemDto(b.Id, b.CreatedAt, b.Items.Count, totalAmount);
        }).ToList();
    }

    public async Task<CreateBillResult> CreateAsync(PurchaseBillSaveDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureValidMasterDataAsync(dto, cancellationToken);

        var bill = new PurchaseBill
        {
            CreatedAt = DateTime.UtcNow,
            Items = dto.Lines.Select(MapLine).ToList()
        };

        await bills.AddAsync(bill, cancellationToken);
        await bills.SaveChangesAsync(cancellationToken);

        var created = await bills.GetByIdWithLinesAsync(bill.Id, cancellationToken)
                      ?? throw new InvalidOperationException("Failed to load purchase bill after save.");

        await audit.AddAsync(new AuditLog
        {
            Entity = "PurchaseBill",
            Action = "Create",
            OldValue = null,
            NewValue = JsonSerializer.Serialize(PurchaseBillMapper.ToResponse(created), JsonOpts),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await audit.SaveChangesAsync(cancellationToken);

        return new CreateBillResult(PurchaseBillMapper.ToResponse(created), WasCreated: true);
    }

    public async Task<PurchaseBillResponseDto?> UpdateAsync(int id, PurchaseBillSaveDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureValidMasterDataAsync(dto, cancellationToken);

        var bill = await bills.GetByIdWithLinesAsync(id, cancellationToken);
        if (bill == null)
            return null;

        var oldSnapshot = JsonSerializer.Serialize(PurchaseBillMapper.ToResponse(bill), JsonOpts);

        var existingLines = bill.Items.ToList();
        bills.RemoveLines(existingLines);
        bill.Items.Clear();

        foreach (var line in dto.Lines)
            bill.Items.Add(MapLine(line));

        await bills.SaveChangesAsync(cancellationToken);

        var updated = await bills.GetByIdWithLinesAsync(id, cancellationToken)
                      ?? throw new InvalidOperationException("Failed to load purchase bill after update.");

        var newSnapshot = JsonSerializer.Serialize(PurchaseBillMapper.ToResponse(updated), JsonOpts);
        await audit.AddAsync(new AuditLog
        {
            Entity = "PurchaseBill",
            Action = "Update",
            OldValue = oldSnapshot,
            NewValue = newSnapshot,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await audit.SaveChangesAsync(cancellationToken);

        return PurchaseBillMapper.ToResponse(updated);
    }

    private static PurchaseBillItem MapLine(PurchaseBillLineInputDto line) => new()
    {
        ItemId = line.ItemId,
        LocationId = line.LocationId,
        Cost = line.Cost,
        Price = line.Price,
        Quantity = line.Quantity,
        Discount = line.DiscountPercent
    };

    private async Task EnsureValidMasterDataAsync(PurchaseBillSaveDto dto, CancellationToken cancellationToken)
    {
        var itemIds = dto.Lines.Select(l => l.ItemId).ToHashSet();
        var locIds = dto.Lines.Select(l => l.LocationId).ToHashSet();

        var existingItems = await items.GetExistingIdsAsync(itemIds, cancellationToken);
        var missingItems = itemIds.Except(existingItems).ToList();
        if (missingItems.Count > 0)
            throw new InvalidOperationException($"Invalid item id(s): {string.Join(", ", missingItems)}");

        var existingLocs = await locations.GetExistingIdsAsync(locIds, cancellationToken);
        var missingLocs = locIds.Except(existingLocs).ToList();
        if (missingLocs.Count > 0)
            throw new InvalidOperationException($"Invalid location id(s): {string.Join(", ", missingLocs)}");
    }
}
