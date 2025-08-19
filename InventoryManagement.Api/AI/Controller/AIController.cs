using InventoryManagement.Api.AI.Models;
using InventoryManagement.Api.AI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.AI.Controllers
{
    /// <summary>
    /// AI Assistant controller for handling AI-powered queries and insights
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIHelperService _aiHelperService;
        private readonly ILogger<AIController> _logger;

        public AIController(
            IAIHelperService aiHelperService,
            ILogger<AIController> logger)
        {
            _aiHelperService = aiHelperService;
            _logger = logger;
        }

        /// <summary>
        /// Ask AI assistant a question about your business
        /// </summary>
        /// <param name="request">The AI query request</param>
        /// <returns>AI-generated response with business insights</returns>
        [HttpPost("ask")]
        public async Task<ActionResult<AIQueryResponse>> AskAsync([FromBody] AIQueryRequest request)
        {
            try
            {
                _logger.LogInformation("AI query received: {Query}", request.Query);

                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest("Query cannot be empty");
                }

                var response = await _aiHelperService.AskAsync(request.Query);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI query: {Query}", request.Query);
                return StatusCode(500, "An error occurred while processing your query");
            }
        }

        /// <summary>
        /// Get suggested questions you can ask the AI
        /// </summary>
        /// <returns>List of suggested questions</returns>
        [HttpGet("suggestions")]
        public async Task<ActionResult<List<string>>> GetSuggestionsAsync()
        {
            try
            {
                var suggestions = await _aiHelperService.GetSuggestedQuestionsAsync();
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI suggestions");
                return StatusCode(500, "An error occurred while getting suggestions");
            }
        }

        /// <summary>
        /// Health check endpoint to verify AI service connectivity
        /// </summary>
        /// <returns>Service status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult<object> HealthCheck()
        {
            return Ok(new
            {
                Service = "Inventory Management AI API",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }
    }
}
