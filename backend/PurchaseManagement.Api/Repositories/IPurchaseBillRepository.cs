using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Repositories;

public interface IPurchaseBillRepository
{
    Task<PurchaseBill?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseBill>> GetAllHeadersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseBill bill, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void RemoveLines(IEnumerable<PurchaseBillItem> lines);
}
