namespace PurchaseManagement.Api.Services;

public static class BillMath
{
    /// <summary>Total cost after discount %: (Cost × Qty) adjusted by discount.</summary>
    public static decimal LineTotalCost(decimal cost, int quantity, decimal discountPercent) =>
        cost * quantity * (1 - discountPercent / 100m);

    public static decimal LineTotalSelling(decimal price, int quantity) =>
        price * quantity;
}
