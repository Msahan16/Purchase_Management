using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseManagement.Api.Entities;

[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Entity")]
    [MaxLength(50)]
    public string Entity { get; set; } = string.Empty;

    [Column("Action")]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [Column("OldValue")]
    public string? OldValue { get; set; }

    [Column("NewValue")]
    public string? NewValue { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
