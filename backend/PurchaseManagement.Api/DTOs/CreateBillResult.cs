namespace PurchaseManagement.Api.DTOs;

public record CreateBillResult(PurchaseBillResponseDto Bill, bool WasCreated);
