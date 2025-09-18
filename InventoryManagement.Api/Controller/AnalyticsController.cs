using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Api.UseCase;
using InventoryManagement.Api.Contracts;

namespace InventoryManagement.Api.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ItemService _itemService;
    private readonly BillService _billService;

    public AnalyticsController(ItemService itemService, BillService billService)
    {
        _itemService = itemService;
        _billService = billService;
    }

    // [HttpPost("sql")]
    // public IActionResult ExecuteSqlQuery([FromBody] ExecuteSqlRequest request)
    // {
    //     var sqlQuery = request.SqlQuery?.Trim();
    //     if (string.IsNullOrWhiteSpace(sqlQuery))
    //     {
    //         return BadRequest("SQL query cannot be empty.");
    //     }

    //     try
    //     {
    //         // Call the SQL analysis skill service to execute the query
    //         var result = _billService.ExecuteSqlQuery(sqlQuery);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest($"Error executing SQL query: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// Gets inventory summary for AI analysis
    /// </summary>
    [HttpGet("inventory-summary")]
    public async Task<IActionResult> GetInventorySummary()
    {
        try
        {
            var items = await _itemService.GetAllAsync();
            
            var summary = new
            {
                TotalItems = items.Count(),
                TotalInventoryValue = items.Sum(i => i.Price * i.Quantity),
                AverageItemValue = items.Any() ? items.Average(i => i.Price) : 0,
                LowStockCount = items.Count(i => i.Quantity <= 10), // Assuming 10 is low stock threshold
                OutOfStockCount = items.Count(i => i.Quantity <= 0),
                LowStockItems = items.Where(i => i.Quantity <= 10 && i.Quantity > 0)
                    .Select(i => new 
                    {
                        ItemName = i.Name,
                        CurrentQuantity = i.Quantity,
                        i.Price
                    }).ToList()
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting inventory summary: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets sales summary for AI analysis
    /// </summary>
    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            // Default to current month if no dates provided
            startDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            endDate ??= DateTime.Now;

            var bills = await _billService.GetAllAsync(startDate, endDate);
            
            // Get detailed bill items for top products analysis
            var topProducts = await GetTopProductsFromBills(bills);
            
            // Calculate growth percentage compared to previous period
            //var growthPercentage = await CalculateGrowthPercentage(startDate.Value, endDate.Value, bills.Sum(b => b.TotalAmount));
            
            var summary = new
            {
                TotalRevenue = bills.Sum(b => b.TotalAmount),
                TotalOrders = bills.Count(),
                AverageOrderValue = bills.Any() ? bills.Average(b => b.TotalAmount) : 0,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TopProducts = topProducts,
                //GrowthPercentage = growthPercentage
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting sales summary: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets top selling products by analyzing individual bills
    /// </summary>
    private async Task<List<object>> GetTopProductsFromBills(IEnumerable<GetAllBillModel> bills)
    {
        var topProducts = new List<object>();
        
        try
        {
            // Since we don't have detailed item info in bills, let's get top items from inventory
            // and cross-reference with bill data for a simplified approach
            var allItems = await _itemService.GetAllAsync();
            var itemSales = allItems.ToDictionary(i => i.Id, i => new
            {
                ItemName = i.Name,
                TotalQuantitySold = 0,
                TotalRevenue = 0m,
                NumberOfOrders = 0,
                AveragePrice = (decimal)i.Price
            });
            

            // For a simplified version, we'll estimate based on available data
            foreach (var bill in bills.Take(50)) // Limit to prevent too many calls
            {
                try
                {
                    var detailedBill = await _billService.GetByIdAsync(bill.Id);
                    if (detailedBill?.BillItems != null)
                    {
                        foreach (var item in detailedBill.BillItems)
                        {
                            if (itemSales.ContainsKey(item.ItemId))
                            {
                                var existing = itemSales[item.ItemId];
                                itemSales[item.ItemId] = new
                                {
                                    existing.ItemName,
                                    TotalQuantitySold = existing.TotalQuantitySold + item.Quantity,
                                    TotalRevenue = existing.TotalRevenue + item.Amount,
                                    NumberOfOrders = existing.NumberOfOrders + 1,
                                    existing.AveragePrice
                                };
                            }
                        }
                    }
                }
                catch
                {
                    // Continue if individual bill fails
                    continue;
                }
            }
            
            // Convert to top products list
            topProducts = itemSales.Values
                .Where(p => p.TotalQuantitySold > 0)
                .OrderByDescending(p => p.TotalRevenue)
                .Take(10)
                .Select(p => (object)new
                {
                    ItemName = p.ItemName,
                    TotalQuantitySold = p.TotalQuantitySold,
                    TotalRevenue = p.TotalRevenue,
                    NumberOfOrders = p.NumberOfOrders,
                    AveragePrice = p.AveragePrice
                })
                .ToList();
        }
        catch
        {
            // Return empty list if there's an error
            return new List<object>();
        }
        
        return topProducts;
    }

    /// <summary>
    /// Gets sales and revenue trends for AI analysis
    /// </summary>
    [HttpGet("sales-trends")]
    public async Task<IActionResult> GetSalesTrends([FromQuery] int months = 6)
    {
        try
        {
            var trends = new List<dynamic>();
            var currentDate = DateTime.Now;
            
            for (int i = months - 1; i >= 0; i--)
            {
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var monthlyBills = await _billService.GetAllAsync(monthStart, monthEnd);
                var monthlyRevenue = monthlyBills.Sum(b => b.TotalAmount);
                var monthlyOrders = monthlyBills.Count();
                var avgOrderValue = monthlyOrders > 0 ? monthlyRevenue / monthlyOrders : 0m;
                
                // Calculate month-over-month growth
                decimal growthRate = 0;
                if (trends.Count > 0) // Not the first month
                {
                    var prevMonthData = trends.Last();
                    var prevRevenue = (decimal)prevMonthData.Revenue;
                    growthRate = prevRevenue > 0 ? Math.Round(((monthlyRevenue - prevRevenue) / prevRevenue) * 100, 2) : 0;
                }
                
                trends.Add(new
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    MonthName = monthStart.ToString("MMMM yyyy"),
                    Revenue = monthlyRevenue,
                    Orders = monthlyOrders,
                    AverageOrderValue = Math.Round(avgOrderValue, 2),
                    GrowthRate = growthRate
                });
            }
            
            // Calculate overall trends
            var totalRevenue = trends.Sum(t => (decimal)t.Revenue);
            var totalOrders = trends.Sum(t => (int)t.Orders);
            var avgMonthlyRevenue = trends.Count > 0 ? totalRevenue / trends.Count : 0;
            
            var result = new
            {
                MonthlyTrends = trends,
                Summary = new
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    AverageMonthlyRevenue = Math.Round(avgMonthlyRevenue, 2),
                    TrendPeriod = $"{months} months",
                    AnalysisDate = DateTime.Now
                }
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting sales trends: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets revenue forecast based on historical data
    /// </summary>
    [HttpGet("revenue-forecast")]
    public async Task<IActionResult> GetRevenueForecast([FromQuery] int historicalMonths = 6, [FromQuery] int forecastMonths = 3)
    {
        try
        {
            // Get historical data
            var historicalData = new List<decimal>();
            var currentDate = DateTime.Now;
            
            for (int i = historicalMonths - 1; i >= 0; i--)
            {
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var monthlyBills = await _billService.GetAllAsync(monthStart, monthEnd);
                var monthlyRevenue = monthlyBills.Sum(b => b.TotalAmount);
                historicalData.Add(monthlyRevenue);
            }
            
            // Simple linear trend forecast
            var forecast = new List<object>();
            var avgGrowthRate = CalculateAverageGrowthRate(historicalData);
            var lastMonthRevenue = historicalData.LastOrDefault();
            
            for (int i = 1; i <= forecastMonths; i++)
            {
                var forecastDate = currentDate.AddMonths(i);
                var forecastRevenue = lastMonthRevenue * (decimal)Math.Pow(1 + (double)avgGrowthRate / 100, i);
                
                forecast.Add(new
                {
                    Month = forecastDate.ToString("yyyy-MM"),
                    MonthName = forecastDate.ToString("MMMM yyyy"),
                    ForecastRevenue = Math.Round(forecastRevenue, 2),
                    Confidence = Math.Max(50, 90 - (i * 10)) // Decreasing confidence over time
                });
            }
            
            var result = new
            {
                HistoricalAverage = Math.Round(historicalData.Average(), 2),
                AverageGrowthRate = Math.Round(avgGrowthRate, 2),
                Forecast = forecast,
                Disclaimer = "Forecast based on historical trends and may not reflect market changes or business decisions."
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating revenue forecast: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculates the average growth rate from historical data
    /// </summary>
    private decimal CalculateAverageGrowthRate(List<decimal> historicalData)
    {
        if (historicalData.Count < 2) return 0;
        
        var growthRates = new List<decimal>();
        for (int i = 1; i < historicalData.Count; i++)
        {
            if (historicalData[i - 1] > 0)
            {
                var growthRate = ((historicalData[i] - historicalData[i - 1]) / historicalData[i - 1]) * 100;
                growthRates.Add(growthRate);
            }
        }
        
        return growthRates.Count > 0 ? growthRates.Average() : 0;
    }

    /// <summary>
    /// Calculates growth percentage compared to the previous period
    /// </summary>
    private async Task<decimal> CalculateGrowthPercentage(DateTime startDate, DateTime endDate, decimal currentRevenue)
{
    try
    {
        // Calculate the previous period (same duration before the current period)
        var periodDuration = endDate - startDate;
        var previousStartDate = startDate - periodDuration;
        var previousEndDate = startDate.AddDays(-1);

        var previousBills = await _billService.GetAllAsync(previousStartDate, previousEndDate);
        var previousRevenue = previousBills.Sum(b => b.TotalAmount);

        if (previousRevenue == 0)
        {
            return currentRevenue > 0 ? 100 : 0; // If no previous revenue, growth is 100% if current > 0
        }

        return Math.Round(((currentRevenue - previousRevenue) / previousRevenue) * 100, 2);
    }
    catch
    {
        // Return 0 if calculation fails
        return 0;
    }
}
    
}
public class ExecuteSqlRequest
{
    public string SqlQuery { get; set; } = string.Empty;
}
