using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Repositories;

public interface ILocationRepository
{
    Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Location?> GetByIdAsync(string locationId, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<string>> GetExistingIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
}
