using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventoryPOSSystem.Models;

public class Inventory
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; } = 10;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LastPurchaseCost { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
