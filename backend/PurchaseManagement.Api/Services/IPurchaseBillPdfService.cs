using PurchaseManagement.Api.DTOs;

namespace PurchaseManagement.Api.Services;

public interface IPurchaseBillPdfService
{
    byte[] GeneratePdf(PurchaseBillResponseDto bill);
}
