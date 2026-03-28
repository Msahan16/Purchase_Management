using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseManagement.Api.Entities;

[Table("PurchaseBillItems")]
public class PurchaseBillItem
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("PurchaseBillId")]
    public int PurchaseBillId { get; set; }

    [ForeignKey(nameof(PurchaseBillId))]
    public PurchaseBill? PurchaseBill { get; set; }

    [Column("ItemId")]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Column("LocationId")]
    [MaxLength(10)]
    public string LocationId { get; set; } = string.Empty;

    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }

    [Column("Cost", TypeName = "decimal(10,2)")]
    public decimal Cost { get; set; }

    [Column("Price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column("Quantity")]
    public int Quantity { get; set; }

    /// <summary>Discount percentage (0–100).</summary>
    [Column("Discount", TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; }
}
