namespace SmartInventoryPOSSystem.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalSales { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSuppliers { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal AnnualRevenue { get; set; }
    public decimal InventoryValue { get; set; }
    public string BestSellingProduct { get; set; } = string.Empty;
    public string MostActiveCustomer { get; set; } = string.Empty;
    public List<LowStockProductViewModel> LowStockProducts { get; set; } = new();
    public List<RecentSaleViewModel> RecentSales { get; set; } = new();
    public List<ChartDataViewModel> MonthlySalesTrend { get; set; } = new();
    public List<ChartDataViewModel> SalesByCategory { get; set; } = new();
    public List<ChartDataViewModel> TopProducts { get; set; } = new();
    public List<ChartDataViewModel> InventoryDistribution { get; set; } = new();
    public List<ChartDataViewModel> TopSuppliers { get; set; } = new();
}

public class LowStockProductViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

public class RecentSaleViewModel
{
    public int SaleId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
}

public class ChartDataViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
