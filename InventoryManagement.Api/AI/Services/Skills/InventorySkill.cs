using InventoryManagement.Api.AI.Models;
using InventoryManagement.Api.Config;
using InventoryManagement.Api.UseCase;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Skill for handling inventory and stock related queries
    /// </summary>
    public class InventorySkill : ISkill
    {
        public SkillType SkillType => SkillType.Inventory;
                private readonly HttpClient _httpClient;
        private readonly InventoryConfig _configuration;
        private readonly ILogger<InventorySkill> _logger;

        private readonly string _baseUrl;
        private readonly string _model;
        private readonly double _temperature;
        private readonly int _maxTokens;
        private readonly string _apiKey;
        private readonly ItemService _itemService;

        public InventorySkill(
            HttpClient httpClient,
            IOptionsSnapshot<InventoryConfig> configuration,
            ILogger<InventorySkill> logger,
            ItemService itemService)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
            _logger = logger;
            _itemService = itemService;

            _baseUrl = _configuration.PerplexityAI.BaseUrl ?? "https://api.perplexity.ai/chat/completions";
            _model = _configuration.PerplexityAI.Model ?? "sonar";
            _temperature = _configuration.PerplexityAI.Temperature;
            _maxTokens = _configuration.PerplexityAI.MaxTokens;
            _apiKey = _configuration.PerplexityAI.ApiKey ?? throw new ArgumentNullException("PerplexityAI:ApiKey is not configured");
        }

        /// <summary>
        /// Executes inventory-specific query processing
        /// </summary>
        public async Task<SkillResponse> ExecuteAsync(SkillRequest request)
        {
            try
            {
                _logger.LogInformation("Processing inventory query: {Query}", request.UserQuery);

                // Get inventory data
                var inventorySummary = await GetInventorySummaryAsync();

                // Special handling for suggestions request
                if (request.UserQuery.ToLower() == "suggestions")
                {
                    var suggestions = GenerateInventorySuggestions(inventorySummary);
                    
                    return new SkillResponse
                    {
                        Response = "Here are some inventory-related questions you can ask:",
                        SuggestedQuestions = suggestions,
                        SkillType = SkillType.Inventory,
                        UsedLocalData = true
                    };
                }
                
                // Create inventory-specific data summary
                var dataSummary = CreateInventoryDataSummary(inventorySummary);
                
                // Create specialized prompt for inventory questions
                var systemPrompt = CreateInventoryPrompt(dataSummary);
                
                // Call AI with inventory context
                var aiResponse = await CallPerplexityAIAsync(systemPrompt, request.UserQuery);
              
                
                // Generate inventory-specific suggestions
                var suggestedQuestions = GenerateInventorySuggestions(inventorySummary);

                return new SkillResponse
                {
                    Response = aiResponse,
                    DataSummary = dataSummary,
                    UsedLocalData = true,
                    SkillType = SkillType.Inventory,
                    SuggestedQuestions = suggestedQuestions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory query: {Query}", request.UserQuery);
                return new SkillResponse
                {
                    Response = "I encountered an error while analyzing your inventory. Please try again later.",
                    UsedLocalData = false,
                    SkillType = SkillType.Inventory
                };
            }
        }

        private async Task<InventorySummaryDto> GetInventorySummaryAsync()
        {
            var items = await _itemService.GetAllAsync();

            var summary = new InventorySummaryDto()
            {
                TotalItems = items.Count(),
                TotalInventoryValue = items.Sum(i => i.Price * i.Quantity),
                AverageItemValue = items.Any() ? items.Average(i => i.Price) : 0,
                LowStockCount = items.Count(i => i.Quantity <= 10), // Assuming 10 is low stock threshold
                OutOfStockCount = items.Count(i => i.Quantity == 0),
                LowStockItems = items.Where(i => i.Quantity <= 10 && i.Quantity > 0)
                    .Select(i => new LowStockItemDto()
                    {
                        ItemName = i.Name,
                        CurrentQuantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
            };
            return summary;
        }

        /// <summary>
        /// Creates inventory-specific data summary
        /// </summary>
        private string CreateInventoryDataSummary(InventorySummaryDto inventory)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"INVENTORY OVERVIEW:");
            summary.AppendLine($"- Total Items: {inventory.TotalItems}");
            summary.AppendLine($"- Total Value: ₹{inventory.TotalInventoryValue:F2}");
            summary.AppendLine($"- Average Item Value: ₹{inventory.AverageItemValue:F2}");
            summary.AppendLine($"- Low Stock Items: {inventory.LowStockCount}");
            summary.AppendLine($"- Out of Stock Items: {inventory.OutOfStockCount}");
            
            if (inventory.LowStockItems.Any())
            {
                summary.AppendLine("\nLOW STOCK ITEMS:");
                foreach (var item in inventory.LowStockItems.Take(5))
                {
                    summary.AppendLine($"- {item.ItemName}: {item.CurrentQuantity} left, ₹{item.Price} each");
                }
            }

            return summary.ToString();
        }

        /// <summary>
        /// Creates inventory-specialized AI prompt
        /// </summary>
        private string CreateInventoryPrompt(string inventoryData)
        {
            return $@"You are an inventory specialist. Analyze this data and provide clear, actionable insights.

{inventoryData}

Rules:
- Be specific and direct
- Use the actual numbers provided
- Prioritize urgent issues (out-of-stock, low stock)
- Keep responses focused and actionable
- Maximum 3-4 sentences

Answer the user's question based on this inventory data.";
        }

        /// <summary>
        /// Generates inventory-specific suggested questions based on current data
        /// </summary>
        private List<string> GenerateInventorySuggestions(InventorySummaryDto inventory)
        {
            var suggestions = new List<string>();

            // Priority suggestions based on current inventory state
            if (inventory.OutOfStockCount > 0)
            {
                suggestions.Add($"What are the {inventory.OutOfStockCount} items that are completely out of stock?");
                suggestions.Add("Which out-of-stock items should I prioritize for immediate reordering?");
            }

            if (inventory.LowStockCount > 0)
            {
                suggestions.Add($"Show me the {inventory.LowStockCount} items running low on stock");
                suggestions.Add("What's the total cost to restock all low inventory items?");
                suggestions.Add("Which low-stock items generate the most revenue?");
            }

            // Value-based suggestions
            if (inventory.TotalInventoryValue > 0)
            {
                suggestions.Add($"What items contribute most to my ₹{inventory.TotalInventoryValue:F0} inventory value?");
                suggestions.Add("Which items have the best value-to-quantity ratio?");
            }

            // General operational suggestions
            suggestions.AddRange(new[]
            {
                "What's my inventory turnover looking like?",
                "How can I optimize my stock levels to reduce carrying costs?",
                "Which items should I consider discontinuing due to slow movement?",
                "What's the ABC analysis of my inventory items?",
                "How much working capital is tied up in inventory?",
                "Which suppliers should I contact for restocking?",
                "What's the seasonal demand pattern for my top items?"
            });

            // Return top 6 most relevant suggestions
            return suggestions.Take(6).ToList();
        }

        /// <summary>
        /// Calls PerplexityAI with inventory-specific context
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

                _logger.LogDebug("Calling PerplexityAI API for inventory query with model: {Model}", _model);
                
                var response = await _httpClient.PostAsync(_baseUrl, httpContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PerplexityAI API returned status: {Status}", response.StatusCode);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error response: {Error}", errorContent);
                    return "I'm having trouble connecting to the AI service. Let me help you with what I can determine from your inventory data.";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var perplexityResponse = JsonConvert.DeserializeObject<PerplexityResponse>(responseContent);

                return perplexityResponse?.choices?.FirstOrDefault()?.message?.content ?? 
                       "I received an empty response from the AI service. Please try rephrasing your question.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PerplexityAI API for inventory query");
                return "I'm currently unable to access external AI services, but I can help you with insights based on your inventory data.";
            }
        }
    }
}
