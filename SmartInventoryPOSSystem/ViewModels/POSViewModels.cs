using SmartInventoryPOSSystem.Models;

namespace SmartInventoryPOSSystem.ViewModels;

public class POSViewModel
{
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
    public List<POSCartItemViewModel> CartItems { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
}

public class POSCartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
