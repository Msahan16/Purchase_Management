using PurchaseManagement.Api.DTOs;

namespace PurchaseManagement.Api.Services;

public interface IPurchaseBillService
{
    Task<PurchaseBillResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseBillListItemDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<CreateBillResult> CreateAsync(PurchaseBillSaveDto dto, CancellationToken cancellationToken = default);
    Task<PurchaseBillResponseDto?> UpdateAsync(int id, PurchaseBillSaveDto dto, CancellationToken cancellationToken = default);
}
