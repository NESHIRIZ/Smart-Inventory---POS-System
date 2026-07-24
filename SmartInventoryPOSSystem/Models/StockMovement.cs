using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventoryPOSSystem.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int QuantityChange { get; set; }

    [Required]
    [StringLength(50)]
    public string MovementType { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? Reference { get; set; }
}
