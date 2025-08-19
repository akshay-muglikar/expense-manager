using InventoryManagement.Api.AI.Models;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Service responsible for routing queries to appropriate skills
    /// </summary>
    public class SkillRouter : ISkillRouter
    {
        private readonly Dictionary<SkillType, ISkill> _skills;
        private readonly QueryClassifier _queryClassifier;
        private readonly ILogger<SkillRouter> _logger;

        public SkillRouter(IEnumerable<ISkill> skills, QueryClassifier queryClassifier, ILogger<SkillRouter> logger)
        {
            _skills = skills?.ToDictionary(s => s.SkillType) ?? new Dictionary<SkillType, ISkill>();
            _queryClassifier = queryClassifier;
            _logger = logger;
        }

        /// <summary>
        /// Routes a query to the most appropriate skill
        /// </summary>
        public async Task<SkillResponse> RouteQueryAsync(string userQuery)
        {
            return await RouteQueryAsync(userQuery, null);
        }

        /// <summary>
        /// Routes a query to the most appropriate skill with client context
        /// </summary>
        public async Task<SkillResponse> RouteQueryAsync(string userQuery, string? clientId)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
            {
                return new SkillResponse
                {
                    Response = "Please provide a question for me to help you with.",
                    SkillType = SkillType.General,
                    UsedLocalData = false
                };
            }

            _logger.LogInformation("Routing query: {Query}", userQuery);

            // Use centralized classifier to determine the best skill
            //var skillType = await _queryClassifier.ClassifyQueryAsync(userQuery);
            var skillType = SkillType.SqlAnalysis;
            if (_skills.TryGetValue(skillType, out var skill))
            {
                _logger.LogInformation("Query routed to {SkillType} skill", skillType);
                
                var skillRequest = new SkillRequest
                {
                    UserQuery = userQuery,
                    ClientId = clientId,
                    SkillType = skillType
                };

                try
                {
                    return await skill.ExecuteAsync(skillRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing {SkillType} skill for query: {Query}", skillType, userQuery);
                    return await CreateGeneralResponse(userQuery);
                }
            }

            // If no specialized skill available, create a general response
            _logger.LogInformation("No specialized skill found for query, using general response");
            return await CreateGeneralResponse(userQuery);
        }

        /// <summary>
        /// Gets all available skills
        /// </summary>
        public IEnumerable<ISkill> GetAvailableSkills()
        {
            return _skills.Values;
        }

        /// <summary>
        /// Gets suggested questions from all skills
        /// </summary>
        public async Task<List<string>> GetSuggestedQuestionsAsync()
        {
            return await GetSuggestedQuestionsAsync(null);
        }

        /// <summary>
        /// Gets suggested questions from all skills with client context
        /// </summary>
        public async Task<List<string>> GetSuggestedQuestionsAsync(string? clientId)
        {
            var allSuggestions = new List<string>();

            // Get suggestions from each skill
            foreach (var skillPair in _skills)
            {
                try
                {
                    // Create a dummy request to get skill-specific suggestions
                    var skillRequest = new SkillRequest
                    {
                        UserQuery = "suggestions",
                        ClientId = clientId,
                        SkillType = skillPair.Key
                    };

                    var response = await skillPair.Value.ExecuteAsync(skillRequest);
                    if (response.SuggestedQuestions != null)
                    {
                        allSuggestions.AddRange(response.SuggestedQuestions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting suggestions from {SkillType} skill", skillPair.Key);
                }
            }

            // Add some contextual general suggestions
            var generalSuggestions = new[]
            {
                "What's my current business performance overview?",
                "Show me my inventory and sales summary",
                "Which products need my immediate attention?",
                "How is my business performing compared to last month?",
                "What are the key metrics I should focus on today?",
                "Help me understand my cash flow from operations",
                "What trends should I be aware of in my business?"
            };

            allSuggestions.AddRange(generalSuggestions);

            // Remove duplicates and return a reasonable number
            return allSuggestions
                .Distinct()
                .OrderBy(x => Guid.NewGuid()) // Randomize order
                .Take(8)
                .ToList();
        }

        /// <summary>
        /// Creates a general response when no specialized skill can handle the query
        /// </summary>
        private Task<SkillResponse> CreateGeneralResponse(string userQuery)
        {
            var generalSuggestions = new List<string>
            {
                "📊 What's my inventory turnover rate?",
                "💰 How much revenue did I generate this month?",
                "⚠️ Which items are critically low in stock?",
                "📈 Show me my best and worst performing products",
                "🎯 What should be my priorities for business growth?",
                "💡 Give me actionable insights for my business",
                "📋 What's my complete business health summary?"
            };

            var response = $"I understand you're asking: \"{userQuery}\"\n\n" +
                          "🤖 **I'm your AI Business Assistant** specializing in:\n\n" +
                          "📦 **Inventory Intelligence**\n" +
                          "   • Stock levels, reorder alerts, and inventory optimization\n" +
                          "   • \"Which items should I restock?\" • \"What's my inventory worth?\"\n\n" +
                          "💸 **Sales & Revenue Analytics**\n" +
                          "   • Revenue trends, customer insights, and growth analysis\n" +
                          "   • \"How are my sales performing?\" • \"What's my profit margin?\"\n\n" +
                          "💡 **Smart Suggestions**: I analyze your actual business data to provide personalized insights and actionable recommendations.\n\n" +
                          "Try asking specific questions about your inventory or sales for detailed analysis!";

            return Task.FromResult(new SkillResponse
            {
                Response = response,
                SkillType = SkillType.General,
                UsedLocalData = false,
                SuggestedQuestions = generalSuggestions
            });
        }
    }
}
