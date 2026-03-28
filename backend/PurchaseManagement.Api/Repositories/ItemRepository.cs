using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Repositories;

public class ItemRepository(AppDbContext db) : IItemRepository
{
    public async Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.Items.AsNoTracking().OrderBy(i => i.ItemName).ToListAsync(cancellationToken);
        return list;
    }

    public Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.ItemId == id, cancellationToken);

    public async Task<IReadOnlySet<int>> GetExistingIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new HashSet<int>();

        var existing = await db.Items.AsNoTracking()
            .Where(i => idList.Contains(i.ItemId))
            .Select(i => i.ItemId)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }
}
