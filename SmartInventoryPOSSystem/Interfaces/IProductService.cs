using SmartInventoryPOSSystem.Models;
using SmartInventoryPOSSystem.ViewModels;

namespace SmartInventoryPOSSystem.Interfaces;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null);
}

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
