namespace InventoryManagement.Api.AI.Models
{
    /// <summary>
    /// DTO for top-selling products
    /// </summary>
    public class TopProductDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal AveragePrice { get; set; }
    }

    /// <summary>
    /// DTO for expense trend analysis
    /// </summary>
    public class ExpenseTrendDto
    {
        public string Period { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageExpenseAmount { get; set; }
    }

    /// <summary>
    /// DTO for inventory summary
    /// </summary>
    public class InventorySummaryDto
    {
        public int TotalItems { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public double AverageItemValue { get; set; }
        public List<LowStockItemDto> LowStockItems { get; set; } = new();
    }

    /// <summary>
    /// DTO for low stock items
    /// </summary>
    public class LowStockItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for sales summary
    /// </summary>
    public class SalesSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public double AverageOrderValue { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<TopProductDto> TopProducts { get; set; } = new();
        public decimal GrowthPercentage { get; set; }
    }

    /// <summary>
    /// DTO for vendor analysis
    /// </summary>
    public class VendorAnalysisDto
    {
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string? ContactInfo { get; set; }
    }

    /// <summary>
    /// DTO for customer insights
    /// </summary>
    public class CustomerInsightDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTimeOffset LastOrderDate { get; set; }
    }
}
