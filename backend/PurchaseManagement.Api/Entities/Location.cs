using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseManagement.Api.Entities;

[Table("Locations")]
public class Location
{
    [Key]
    [Column("location_id")]
    [MaxLength(10)]
    public string LocationId { get; set; } = string.Empty;

    [Column("location_name")]
    [MaxLength(100)]
    public string LocationName { get; set; } = string.Empty;
}
