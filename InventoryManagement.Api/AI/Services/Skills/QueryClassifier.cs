using InventoryManagement.Api.AI.Models;
using Newtonsoft.Json;
using System.Text;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Centralized query classifier that determines which skill should handle a query
    /// Uses hybrid approach: keyword matching first, then single AI call if needed
    /// </summary>
    public class QueryClassifier
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QueryClassifier> _logger;
        
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly string _apiKey;
        
        // Pre-defined patterns for fast classification
        private readonly Dictionary<SkillType, string[]> _skillKeywords;

        public QueryClassifier(HttpClient httpClient, IConfiguration configuration, ILogger<QueryClassifier> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _baseUrl = _configuration["PerplexityAI:BaseUrl"] ?? "https://api.perplexity.ai/chat/completions";
            _model = _configuration["PerplexityAI:Model"] ?? "sonar";
            _apiKey = _configuration["PerplexityAI:ApiKey"] ?? throw new ArgumentNullException("PerplexityAI:ApiKey is not configured");
            
            _skillKeywords = new Dictionary<SkillType, string[]>
            {
                [SkillType.Inventory] = new[]
                {
                    "inventory", "stock", "items", "products", "quantity", "available",
                    "left", "remaining", "reorder", "low stock", "out of stock",
                    "warehouses", "supplies", "materials", "goods", "sku"
                },
                [SkillType.Billing] = new[]
                {
                    "bill", "billing", "sales", "revenue", "payment", "invoice", "customer",
                    "profit", "loss", "earnings", "income", "total sales", "monthly sales",
                    "purchase", "expenses", "cost", "margin", "roi", "return", "financial",
                    "accounting", "transaction", "receipt", "paid", "unpaid", "due"
                }
            };
        }

        /// <summary>
        /// Classifies query and returns the most appropriate skill type
        /// Uses AI classification first, with keyword matching as fallback only
        /// </summary>
        public async Task<SkillType> ClassifyQueryAsync(string userQuery)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
                return SkillType.General;

            // Step 1: Primary AI-based classification
            try
            {
                var aiClassification = await ClassifyWithAIAsync(userQuery);
                _logger.LogDebug("Query classified using AI: {Skill}", aiClassification);
                return aiClassification;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI classification failed, falling back to keyword matching");
            }

            // Step 2: Fallback to keyword matching only if AI fails
            var keywordResult = GetKeywordFallbackClassification(userQuery);
            _logger.LogDebug("Query classified using keyword fallback: {Skill}", keywordResult);
            return keywordResult;
        }

        /// <summary>
        /// Simple keyword-based fallback classification when AI fails
        /// </summary>
        private SkillType GetKeywordFallbackClassification(string userQuery)
        {
            var query = userQuery.ToLowerInvariant();

            // SQL Analysis keywords (check first for specificity)
            var sqlKeywords = new[] { "show me", "list all", "find", "search", "analyze", "compare", "top", "bottom", "highest", "lowest", "custom", "detailed", "specific" };
            if (sqlKeywords.Any(keyword => query.Contains(keyword)))
            {
                return SkillType.SqlAnalysis;
            }

            // Trends keywords (check next for specificity)
            var trendKeywords = new[] { "trend", "growth", "forecast", "over time", "monthly", "quarterly", "year over year", "historical", "pattern", "projection" };
            if (trendKeywords.Any(keyword => query.Contains(keyword)))
            {
                return SkillType.Trends;
            }

            // Billing keywords
            var billingKeywords = new[] { "sales", "revenue", "billing", "invoice", "payment", "profit", "order", "customer", "bill" };
            if (billingKeywords.Any(keyword => query.Contains(keyword)))
            {
                return SkillType.Billing;
            }

            // Inventory keywords
            var inventoryKeywords = new[] { "inventory", "stock", "items", "products", "warehouse", "quantity", "supply" };
            if (inventoryKeywords.Any(keyword => query.Contains(keyword)))
            {
                return SkillType.Inventory;
            }

            return SkillType.General;
        }

        /// <summary>
        /// Uses AI to classify queries with enhanced prompting
        /// </summary>
        private async Task<SkillType> ClassifyWithAIAsync(string userQuery)
        {
            var prompt = @$"You are a business query classifier for an inventory management system.

Classify this query into exactly ONE category:

Query: ""{userQuery}""

Categories and their scope:
• SQLANALYSIS - detailed analysis, custom queries, specific data requests, show me, list all, find, search, analyze, compare, top/bottom, detailed reporting
• TRENDS - trend analysis, growth patterns, historical comparisons, forecasting, month-over-month, year-over-year, performance over time, sales trends, revenue trends
• INVENTORY - stock levels, quantities, products, items, reordering, warehouses, supplies, out-of-stock, low stock
• BILLING - current sales, revenue totals, payments, invoices, orders, customers, profits, financial performance, billing
• GENERAL - everything else including greetings, help requests, unclear queries

Examples:
- ""Show me my top 10 selling products"" → SQLANALYSIS
- ""Find customers with highest purchases"" → SQLANALYSIS
- ""Show me sales trends over 6 months"" → TRENDS
- ""What's my revenue growth this year?"" → TRENDS
- ""What items are out of stock?"" → INVENTORY  
- ""How much revenue did I make this month?"" → BILLING
- ""Help me understand my business"" → GENERAL

Respond with exactly one word: SQLANALYSIS, TRENDS, INVENTORY, BILLING, or GENERAL";

            var request = new PerplexityRequest
            {
                model = _model,
                temperature = 0, // Deterministic for classification
                max_tokens = 5, // Very short response
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
            var result = perplexityResponse?.choices?.FirstOrDefault()?.message?.content?.Trim().ToUpper();

            return result switch
            {
                "SQLANALYSIS" => SkillType.SqlAnalysis,
                "TRENDS" => SkillType.Trends,
                "INVENTORY" => SkillType.Inventory,
                "BILLING" => SkillType.Billing,
                _ => SkillType.General
            };
        }


    }
}
