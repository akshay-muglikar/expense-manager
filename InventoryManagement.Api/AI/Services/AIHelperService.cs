using InventoryManagement.Api.AI.Models;
using InventoryManagement.Api.AI.Services.Skills;
using Newtonsoft.Json;
using System.Text;

namespace InventoryManagement.Api.AI.Services
{
    /// <summary>
    /// Main AI Helper Service that uses skills-based routing for specialized query handling
    /// </summary>
    public interface IAIHelperService
    {
        Task<AIQueryResponse> AskAsync(string userQuery);
        Task<List<string>> GetSuggestedQuestionsAsync();
    }

    public class AIHelperService : IAIHelperService
    {
        private readonly ISkillRouter _skillRouter;
        private readonly ILogger<AIHelperService> _logger;

        public AIHelperService(
            ISkillRouter skillRouter,
            ILogger<AIHelperService> logger)
        {
            _skillRouter = skillRouter;
            _logger = logger;
        }

        /// <summary>
        /// Processes user query using skills-based routing
        /// </summary>
        public async Task<AIQueryResponse> AskAsync(string userQuery)
        {
            try
            {
                _logger.LogInformation("Processing AI query: {Query}", userQuery);

                // Route the query to appropriate skill
                var skillResponse = await _skillRouter.RouteQueryAsync(userQuery);

                return new AIQueryResponse
                {
                    Response = skillResponse.Response,
                    LocalDataSummary = skillResponse.DataSummary ?? "No specific data context used",
                    UsedLocalData = skillResponse.UsedLocalData,
                    SuggestedQuestions = skillResponse.SuggestedQuestions ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI query: {Query}", userQuery);
                return new AIQueryResponse
                {
                    Response = "I encountered an error while processing your query. Please try again later.",
                    UsedLocalData = false
                };
            }
        }

        /// <summary>
        /// Gets suggested questions using skill-based routing
        /// </summary>
        public async Task<List<string>> GetSuggestedQuestionsAsync()
        {
          return new List<string>
            {
                // Trends Analysis questions - NEW!
                "Show me my sales trends over the last 6 months",
                "What's my month-over-month revenue growth rate?",
                "How is my business performing compared to previous quarters?",
                
                // Inventory-focused questions based on InventorySkill prompts
                "What are my out-of-stock items that need immediate attention?",
                "Which items are running low and should I reorder first?",
                "What's the total value of my current inventory?",
                
                // Sales/Billing questions based on BillingSkill prompts  
                "How much revenue did I generate this month?",
                "Which products are my top revenue generators?",
                "What's my profit margin analysis by product?",
                "Show me customer purchasing behavior insights",
                
                // Business analytics questions combining multiple skills
                "What's my complete business performance overview?",
            };
            
        }
    }
}
