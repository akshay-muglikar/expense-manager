using InventoryManagement.Api.AI.Models;
using InventoryManagement.Api.Config;
using InventoryManagement.Api.Contracts;
using InventoryManagement.Api.UseCase;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Skill for handling billing and sales related queries
    /// </summary>
    public class BillingSkill : ISkill
    {
        public SkillType SkillType => SkillType.Billing;
        
        private readonly HttpClient _httpClient;
        private readonly InventoryConfig _configuration;
        private readonly ILogger<BillingSkill> _logger;

        private readonly string _baseUrl;
        private readonly string _model;
        private readonly double _temperature;
        private readonly int _maxTokens;
        private readonly string _apiKey;

        private readonly BillService _billService;
        private readonly ItemService _itemService;
        public BillingSkill(
            HttpClient httpClient,
            IOptionsSnapshot<InventoryConfig> configuration,
            ILogger<BillingSkill> logger,
            BillService billService,
            ItemService itemService)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
            _logger = logger;
            _billService = billService;
            _itemService = itemService;

            _baseUrl = _configuration.PerplexityAI.BaseUrl ?? "https://api.perplexity.ai/chat/completions";
            _model = _configuration.PerplexityAI.Model ?? "sonar";
            _temperature = _configuration.PerplexityAI.Temperature;
            _maxTokens = _configuration.PerplexityAI.MaxTokens;
            _apiKey = _configuration.PerplexityAI.ApiKey ?? throw new ArgumentNullException("PerplexityAI:ApiKey is not configured");
        }

        /// <summary>
        /// Executes billing-specific query processing
        /// </summary>
        public async Task<SkillResponse> ExecuteAsync(SkillRequest request)
        {
            try
            {
                _logger.LogInformation("Processing billing query: {Query}", request.UserQuery);

                // Get billing data
                var billingSummary = await GetSalesSummaryAsync();

                
                // Create billing-specific data summary
                var dataSummary = CreateBillingDataSummary(billingSummary);
                
                // Create specialized prompt for billing questions
                var systemPrompt = CreateBillingPrompt(dataSummary);
                
                // Call AI with billing context
                var aiResponse = await CallPerplexityAIAsync(systemPrompt, request.UserQuery);
                
                // Generate billing-specific suggestions
                var suggestedQuestions = GenerateBillingSuggestions(billingSummary);

                return new SkillResponse
                {
                    Response = aiResponse,
                    DataSummary = dataSummary,
                    UsedLocalData = true,
                    SkillType = SkillType.Billing,
                    SuggestedQuestions = suggestedQuestions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing billing query: {Query}", request.UserQuery);
                return new SkillResponse
                {
                    Response = "I encountered an error while analyzing your billing data. Please try again later.",
                    UsedLocalData = false,
                    SkillType = SkillType.Billing
                };
            }
        }

        private async Task<SalesSummaryDto> GetSalesSummaryAsync()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = DateTime.Now;

            var bills = await _billService.GetAllAsync(startDate, endDate);

            // Get detailed bill items for top products analysis
            var topProducts = await GetTopProductsFromBills(bills);

            // Calculate growth percentage compared to previous period
            var growthPercentage = await CalculateGrowthPercentage(startDate, endDate, bills.Sum(b => b.TotalAmount));

            var summary = new SalesSummaryDto()
            {
                TotalRevenue = bills.Sum(b => b.TotalAmount),
                TotalOrders = bills.Count(),
                AverageOrderValue = bills.Any() ? bills.Average(b => b.TotalAmount) : 0,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TopProducts = topProducts,
                GrowthPercentage = growthPercentage
            };
            return summary;
        }
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
   
   private async Task<List<TopProductDto>> GetTopProductsFromBills(IEnumerable<GetAllBillModel> bills)
        {
            var topProducts = new List<TopProductDto>();

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
                    .Select(p => new TopProductDto()
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
                return new List<TopProductDto>();
            }

            return topProducts;
        }

        /// <summary>
        /// Creates billing-specific data summary
        /// </summary>
        private string CreateBillingDataSummary(SalesSummaryDto sales)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"BILLING & SALES OVERVIEW:");
            summary.AppendLine($"- Total Orders: {sales.TotalOrders}");
            summary.AppendLine($"- Total Revenue: ₹{sales.TotalRevenue:F2}");
            summary.AppendLine($"- Average Order Value: ₹{sales.AverageOrderValue:F2}");
            summary.AppendLine($"- Period: {sales.PeriodStart:yyyy-MM-dd} to {sales.PeriodEnd:yyyy-MM-dd}");
            
            if (sales.GrowthPercentage != 0)
            {
                summary.AppendLine($"- Growth: {sales.GrowthPercentage:F1}%");
            }
            
            if (sales.TopProducts.Any())
            {
                summary.AppendLine("\nTOP SELLING PRODUCTS:");
                foreach (var product in sales.TopProducts.Take(5))
                {
                    summary.AppendLine($"- {product.ItemName}: {product.TotalQuantitySold} units, ₹{product.TotalRevenue:F2}");
                }
            }

            return summary.ToString();
        }

        /// <summary>
        /// Creates billing-specialized AI prompt
        /// </summary>
        private string CreateBillingPrompt(string billingData)
        {
            return $@"You are a sales and billing specialist. Analyze this data and provide clear, actionable insights.

{billingData}

Rules:
- Be specific and direct
- Use the actual numbers provided
- Focus on revenue trends and growth opportunities
- Keep responses focused and actionable
- Maximum 3-4 sentences

Answer the user's question based on this sales data.";
        }

        /// <summary>
        /// Generates billing-specific suggested questions based on current data
        /// </summary>
        private List<string> GenerateBillingSuggestions(SalesSummaryDto sales)
        {
            var suggestions = new List<string>();

            // Revenue and growth focused suggestions
            if (sales.TotalRevenue > 0)
            {
                suggestions.Add($"How can I grow beyond my current ₹{sales.TotalRevenue:F0} monthly revenue?");
                suggestions.Add("What's driving my revenue growth this month?");
                
                if (sales.GrowthPercentage != 0)
                {
                    var trend = sales.GrowthPercentage > 0 ? "growth" : "decline";
                    suggestions.Add($"What factors contributed to my {Math.Abs(sales.GrowthPercentage):F1}% revenue {trend}?");
                }
            }

            // Performance and strategy suggestions
            if (sales.TotalOrders > 0)
            {
                var avgOrder = sales.TotalRevenue / sales.TotalOrders;
                suggestions.Add($"How can I increase my ₹{avgOrder:F0} average order value?");
                suggestions.Add($"What strategies can convert more of my {sales.TotalOrders} orders into repeat customers?");
            }

            // Product performance suggestions
            if (sales.TopProducts.Any())
            {
                suggestions.Add("Which products have the highest profit margins?");
                suggestions.Add("How can I cross-sell with my best-performing products?");
                suggestions.Add("What inventory should I stock more based on sales performance?");
            }

            // Analytics and insights suggestions
            suggestions.AddRange(new[]
            {
                "What are my peak sales hours and days?",
                "Which customer segments generate the most revenue?",
                "How does my current month compare to the same period last year?",
                "What's my sales forecast for next month?",
                "Which products should I discontinue due to poor sales?",
                "What payment methods do my customers prefer?",
                "How can I optimize my pricing strategy?"
            });

            // Return top 6 most relevant suggestions
            return suggestions.Take(6).ToList();
        }

        /// <summary>
        /// Calls PerplexityAI with billing-specific context
        /// </summary>
        private async Task<string> CallPerplexityAIAsync(string systemPrompt, string userQuery)
        {
            try
            {
                var request = new PerplexityRequest
                {
                    model = _model,
                    temperature = _temperature,
                    max_tokens = _maxTokens,
                    messages = new List<PerplexityMessage>
                    {
                        new() { role = "system", content = systemPrompt },
                        new() { role = "user", content = userQuery }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(request);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogDebug("Calling PerplexityAI API for billing query with model: {Model}", _model);
                
                var response = await _httpClient.PostAsync(_baseUrl, httpContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PerplexityAI API returned status: {Status}", response.StatusCode);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error response: {Error}", errorContent);
                    return "I'm having trouble connecting to the AI service. Let me help you with what I can determine from your billing data.";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var perplexityResponse = JsonConvert.DeserializeObject<PerplexityResponse>(responseContent);

                return perplexityResponse?.choices?.FirstOrDefault()?.message?.content ?? 
                       "I received an empty response from the AI service. Please try rephrasing your question.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PerplexityAI API for billing query");
                return "I'm currently unable to access external AI services, but I can help you with insights based on your billing data.";
            }
        }
    }
}
