using Microsoft.EntityFrameworkCore;
using SmartInventoryPOSSystem.Data;
using SmartInventoryPOSSystem.Interfaces;
using SmartInventoryPOSSystem.Models;
using SmartInventoryPOSSystem.ViewModels;

namespace SmartInventoryPOSSystem.Services;

public class ProductService : IProductService, IDashboardService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.SKU.Contains(searchTerm) ||
                (p.Barcode != null && p.Barcode.Contains(searchTerm)));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null)
    {
        return !await _context.Products.AnyAsync(p => p.SKU == sku && (!excludeId.HasValue || p.Id != excludeId.Value));
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var startOfDay = now.Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        var productCount = await _context.Products.CountAsync();
        var saleCount = await _context.Sales.CountAsync();
        var todayRevenue = await _context.Sales
            .Where(s => s.SaleDate >= startOfDay)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var monthlyRevenue = await _context.Sales
            .Where(s => s.SaleDate >= startOfMonth)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var annualRevenue = await _context.Sales
            .Where(s => s.SaleDate >= startOfYear)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

        var totalCustomers = await _context.Customers.CountAsync();
        var totalSuppliers = await _context.Suppliers.CountAsync();
        var lowStockCount = await _context.Products.CountAsync(p => p.StockQuantity <= 10);
        var outOfStockCount = await _context.Products.CountAsync(p => p.StockQuantity == 0);
        var inventoryValue = await _context.Products.SumAsync(p => p.Price * p.StockQuantity);

        var bestSellingProductGroup = await _context.SaleItems
            .Include(si => si.Product)
            .Where(si => si.Product != null)
            .GroupBy(si => new { si.ProductId, si.Product!.Name })
            .Select(g => new
            {
                Name = g.Key.Name,
                Quantity = g.Sum(si => si.Quantity)
            })
            .OrderByDescending(g => g.Quantity)
            .FirstOrDefaultAsync();

        var mostActiveCustomerGroup = await _context.Sales
            .Include(s => s.Customer)
            .Where(s => s.Customer != null)
            .GroupBy(s => new { s.CustomerId, s.Customer!.FullName })
            .Select(g => new
            {
                Name = g.Key.FullName,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .FirstOrDefaultAsync();

        var topProducts = await _context.SaleItems
            .Include(si => si.Product)
            .Where(si => si.Product != null)
            .GroupBy(si => new { si.ProductId, si.Product!.Name })
            .Select(g => new ChartDataViewModel
            {
                Label = g.Key.Name,
                Value = g.Sum(si => si.Quantity)
            })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync();

        var topSuppliers = await _context.SaleItems
            .Include(si => si.Product)
            .ThenInclude(p => p.Supplier)
            .Where(si => si.Product != null && si.Product.Supplier != null)
            .GroupBy(si => new { si.Product.Supplier!.Id, si.Product.Supplier!.Name })
            .Select(g => new ChartDataViewModel
            {
                Label = g.Key.Name,
                Value = g.Sum(si => si.TotalPrice)
            })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync();

        var salesByCategory = await _context.SaleItems
            .Include(si => si.Product)
            .ThenInclude(p => p.Category)
            .Where(si => si.Product != null && si.Product.Category != null)
            .GroupBy(si => new { si.Product.Category!.Id, si.Product.Category!.Name })
            .Select(g => new ChartDataViewModel
            {
                Label = g.Key.Name,
                Value = g.Sum(si => si.TotalPrice)
            })
            .OrderByDescending(x => x.Value)
            .ToListAsync();

        var inventoryDistribution = await _context.Products
            .Include(p => p.Category)
            .GroupBy(p => new { p.CategoryId, p.Category!.Name })
            .Select(g => new ChartDataViewModel
            {
                Label = g.Key.Name,
                Value = g.Sum(p => p.StockQuantity)
            })
            .OrderByDescending(x => x.Value)
            .ToListAsync();

        var monthlySalesTrend = await _context.Sales
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new ChartDataViewModel
            {
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Value = g.Sum(s => s.TotalAmount)
            })
            .ToListAsync();

        var lowStockProducts = await _context.Products
            .Where(p => p.StockQuantity <= 10)
            .Select(p => new LowStockProductViewModel
            {
                ProductName = p.Name,
                StockQuantity = p.StockQuantity
            })
            .OrderBy(p => p.StockQuantity)
            .Take(5)
            .ToListAsync();

        var recentSales = await _context.Sales
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Product)
            .Include(s => s.Customer)
            .OrderByDescending(s => s.SaleDate)
            .Take(5)
            .ToListAsync();

        var mappedRecentSales = recentSales.Select(s => new RecentSaleViewModel
        {
            SaleId = s.Id,
            ProductName = s.SaleItems.FirstOrDefault()?.Product?.Name ?? "-",
            CustomerName = s.Customer?.FullName ?? "Walk-in",
            TotalAmount = s.TotalAmount,
            SaleDate = s.SaleDate
        }).ToList();

        return new DashboardViewModel
        {
            TotalProducts = productCount,
            TotalSales = saleCount,
            TotalCustomers = totalCustomers,
            TotalSuppliers = totalSuppliers,
            PendingOrders = 0,
            CompletedOrders = saleCount,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            TodayRevenue = todayRevenue,
            MonthlyRevenue = monthlyRevenue,
            AnnualRevenue = annualRevenue,
            InventoryValue = inventoryValue,
            BestSellingProduct = bestSellingProductGroup?.Name ?? "N/A",
            MostActiveCustomer = mostActiveCustomerGroup?.Name ?? "N/A",
            TopProducts = topProducts,
            TopSuppliers = topSuppliers,
            SalesByCategory = salesByCategory,
            InventoryDistribution = inventoryDistribution,
            MonthlySalesTrend = monthlySalesTrend,
            LowStockProducts = lowStockProducts,
            RecentSales = mappedRecentSales
        };
    }
}
