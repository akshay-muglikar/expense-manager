namespace InventoryManagement.Api.AI.Models
{
    /// <summary>
    /// Request model for AI queries
    /// </summary>
    public class AIQueryRequest
    {
        /// <summary>
        /// User's question or query
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Optional context for the query
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// Whether to include local data analysis
        /// </summary>
        public bool IncludeLocalData { get; set; } = true;
    }

    /// <summary>
    /// Response model for AI queries
    /// </summary>
    public class AIQueryResponse
    {
        /// <summary>
        /// AI-generated response
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Local data summary that was provided to AI
        /// </summary>
        public string? LocalDataSummary { get; set; }

        /// <summary>
        /// Whether local data was included in the analysis
        /// </summary>
        public bool UsedLocalData { get; set; }

        /// <summary>
        /// Response timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Suggested follow-up questions
        /// </summary>
        public List<string> SuggestedQuestions { get; set; } = new();
    }
}
