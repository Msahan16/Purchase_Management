using PurchaseManagement.Api.DTOs;

namespace PurchaseManagement.Api.Services;

public interface IMasterDataService
{
    Task<IReadOnlyList<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default);
}
