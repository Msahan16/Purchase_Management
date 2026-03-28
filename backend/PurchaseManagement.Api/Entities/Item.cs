using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseManagement.Api.Entities;

[Table("Items")]
public class Item
{
    [Key]
    [Column("item_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ItemId { get; set; }

    [Column("item_name")]
    [MaxLength(100)]
    public string ItemName { get; set; } = string.Empty;
}
