using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Repositories;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) =>
        await db.AuditLogs.AddAsync(log, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
