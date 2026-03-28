using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Repositories;

namespace PurchaseManagement.Api.Services;

public class MasterDataService(IItemRepository items, ILocationRepository locations) : IMasterDataService
{
    public async Task<IReadOnlyList<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await items.GetAllAsync(cancellationToken);
        return rows.Select(i => new ItemDto(i.ItemId, i.ItemName)).ToList();
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await locations.GetAllAsync(cancellationToken);
        return rows.Select(l => new LocationDto(l.LocationId, l.LocationName)).ToList();
    }
}
