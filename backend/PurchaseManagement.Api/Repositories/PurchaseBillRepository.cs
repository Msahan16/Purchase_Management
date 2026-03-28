using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Repositories;

public class PurchaseBillRepository(AppDbContext db) : IPurchaseBillRepository
{
    public Task<PurchaseBill?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default) =>
        db.PurchaseBills
            .Include(b => b.Items)
            .ThenInclude(i => i.Item)
            .Include(b => b.Items)
            .ThenInclude(i => i.Location)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PurchaseBill>> GetAllHeadersAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.PurchaseBills
            .AsNoTracking()
            .Include(b => b.Items)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task AddAsync(PurchaseBill bill, CancellationToken cancellationToken = default)
    {
        await db.PurchaseBills.AddAsync(bill, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public void RemoveLines(IEnumerable<PurchaseBillItem> lines) =>
        db.PurchaseBillItems.RemoveRange(lines);
}
