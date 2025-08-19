using InventoryManagement.Api.AI.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;
using InventoryManagement.Api.Config;
using InventoryManagement.Api.UseCase;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Skill for handling sales and revenue trend analysis queries
    /// </summary>
    public class TrendsSkill : ISkill
    {
        public SkillType SkillType => SkillType.Trends;

        private readonly HttpClient _httpClient;
        private readonly BillService _billService;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly ILogger<TrendsSkill> _logger;

        public TrendsSkill(
            HttpClient httpClient,
            IOptionsSnapshot<InventoryConfig> configuration,
            ILogger<TrendsSkill> logger,
            BillService billService)
        {
            _httpClient = httpClient;
            _apiKey = configuration.Value.PerplexityAI.ApiKey ?? throw new ArgumentNullException("PerplexityAI:ApiKey");
            _baseUrl = configuration.Value.PerplexityAI.BaseUrl ?? "https://api.perplexity.ai/chat/completions";
            _model = configuration.Value.PerplexityAI.Model ?? "llama-3.1-sonar-small-128k-online";
            _logger = logger;
            _billService = billService;
        }

        /// <summary>
        /// Executes trend analysis queries
        /// </summary>
        public async Task<SkillResponse> ExecuteAsync(SkillRequest request)
        {
            if (request.UserQuery.ToLower().Contains("suggestions"))
            {
                return GetSuggestedQuestions();
            }

            try
            {
                _logger.LogInformation("Executing trends analysis for query: {Query}", request.UserQuery);

                // Get sales trends data
                var trendsData = await GetSalesTrendsAsync();
                var forecastData = await GetRevenueForecastAsync();

                if (trendsData == null)
                {
                    return new SkillResponse
                    {
                        Response = "I'm sorry, I couldn't retrieve sales trend data at the moment. Please try again later.",
                        UsedLocalData = false,
                        SkillType = SkillType.Trends
                    };
                }

                // Create comprehensive data summary for AI
                var dataSummary = CreateTrendsDataSummary(trendsData, forecastData);

                // Generate AI response using trend data
                var aiResponse = await GenerateAIResponse(request.UserQuery, dataSummary);

                return new SkillResponse
                {
                    Response = aiResponse,
                    DataSummary = dataSummary,
                    UsedLocalData = true,
                    SkillType = SkillType.Trends,
                    SuggestedQuestions = GetRelatedQuestions()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing trends skill for query: {Query}", request.UserQuery);
                return new SkillResponse
                {
                    Response = "I encountered an error while analyzing sales trends. Please try rephrasing your question or contact support if the issue persists.",
                    UsedLocalData = false,
                    SkillType = SkillType.Trends
                };
            }
        }

        private async Task<dynamic> GetSalesTrendsAsync()
        {
            int months = 6;
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

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

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
        private async Task<dynamic?> GetRevenueForecastAsync()
        {
            int historicalMonths = 6;int forecastMonths = 3;
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
            return result;
        }

        /// <summary>
        /// Creates a comprehensive data summary for AI consumption
        /// </summary>
        private string CreateTrendsDataSummary(dynamic trendsData, dynamic? forecastData)
        {
            var summary = new StringBuilder();
            summary.AppendLine("=== SALES & REVENUE TRENDS ANALYSIS ===");
            
            try
            {
                // Monthly trends
                if (trendsData.MonthlyTrends != null)
                {
                    summary.AppendLine("\n--- MONTHLY PERFORMANCE ---");
                    foreach (var month in trendsData.MonthlyTrends)
                    {
                        summary.AppendLine($"{month.MonthName}: Revenue ${month.Revenue:N2}, Orders: {month.Orders}, Avg Order: ${month.AverageOrderValue:N2}, Growth: {month.GrowthRate:N1}%");
                    }
                }

                // Overall summary
                if (trendsData.Summary != null)
                {
                    summary.AppendLine($"\n--- OVERALL SUMMARY ---");
                    summary.AppendLine($"Total Revenue: ${trendsData.Summary.TotalRevenue:N2}");
                    summary.AppendLine($"Total Orders: {trendsData.Summary.TotalOrders:N0}");
                    summary.AppendLine($"Average Monthly Revenue: ${trendsData.Summary.AverageMonthlyRevenue:N2}");
                    summary.AppendLine($"Analysis Period: {trendsData.Summary.TrendPeriod}");
                }

                // Forecast data
                if (forecastData?.Forecast != null)
                {
                    summary.AppendLine($"\n--- REVENUE FORECAST ---");
                    summary.AppendLine($"Historical Average: ${forecastData.HistoricalAverage:N2}");
                    summary.AppendLine($"Average Growth Rate: {forecastData.AverageGrowthRate:N2}%");
                    
                    foreach (var forecast in forecastData.Forecast)
                    {
                        summary.AppendLine($"{forecast.MonthName}: ${forecast.ForecastRevenue:N2} (Confidence: {forecast.Confidence}%)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating trends data summary");
                summary.AppendLine("Error processing trend data details.");
            }
            
            return summary.ToString();
        }

        /// <summary>
        /// Generates AI response using Perplexity AI with trend data
        /// </summary>
        private async Task<string> GenerateAIResponse(string userQuery, string dataSummary)
        {
            var prompt = $@"You are a business intelligence analyst helping interpret sales and revenue trends.

User Question: {userQuery}

Current Business Trend Data:
{dataSummary}

Instructions:
- Analyze the trend data to answer the user's question specifically
- Identify key patterns, growth rates, and business insights
- Highlight concerning trends or positive developments
- Provide actionable recommendations based on the data
- Use specific numbers and percentages from the data
- Keep the response conversational but professional
- If forecasting, mention confidence levels and assumptions

Focus on sales performance, revenue growth, order patterns, and business trajectory.";

            var request = new PerplexityRequest
            {
                model = _model,
                temperature = 0.3,
                max_tokens = 500,
                messages = new List<PerplexityMessage>
                {
                    new() { role = "user", content = prompt }
                }
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl, httpContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var perplexityResponse = JsonConvert.DeserializeObject<PerplexityResponse>(responseContent);

            return perplexityResponse?.choices?.FirstOrDefault()?.message?.content?.Trim() 
                   ?? "I was able to retrieve your sales trend data, but I'm having trouble generating insights right now. Please try again.";
        }

        /// <summary>
        /// Gets trend-related suggested questions
        /// </summary>
        private List<string> GetRelatedQuestions()
        {
            return new List<string>
            {
                "What are my sales trends over the last 6 months?",
                "How is my monthly revenue performing compared to previous months?",
                "What's my month-over-month growth rate?",
                "Show me revenue forecast for next quarter",
                "Which months were my strongest sales periods?",
                "What patterns do you see in my business growth?",
                "How consistent is my revenue stream?",
                "What should I expect for next month's revenue?"
            };
        }

        /// <summary>
        /// Returns suggested questions for trends analysis
        /// </summary>
        private SkillResponse GetSuggestedQuestions()
        {
            return new SkillResponse
            {
                Response = "Here are some questions I can help you with regarding sales and revenue trends:",
                SuggestedQuestions = GetRelatedQuestions(),
                UsedLocalData = false,
                SkillType = SkillType.Trends
            };
        }
    }
}
