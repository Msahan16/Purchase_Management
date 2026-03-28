using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Repositories;

public class LocationRepository(AppDbContext db) : ILocationRepository
{
    public async Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.Locations.AsNoTracking().OrderBy(l => l.LocationId).ToListAsync(cancellationToken);
        return list;
    }

    public Task<Location?> GetByIdAsync(string locationId, CancellationToken cancellationToken = default) =>
        db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.LocationId == locationId, cancellationToken);

    public async Task<IReadOnlySet<string>> GetExistingIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new HashSet<string>();

        var existing = await db.Locations.AsNoTracking()
            .Where(l => idList.Contains(l.LocationId))
            .Select(l => l.LocationId)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }
}
