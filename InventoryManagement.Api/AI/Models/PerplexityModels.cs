namespace InventoryManagement.Api.AI.Models
{
    /// <summary>
    /// PerplexityAI API request model
    /// </summary>
    public class PerplexityRequest
    {
        public string model { get; set; } = "llama-3.1-sonar-small-128k-online";
        public List<PerplexityMessage> messages { get; set; } = new();
        public double temperature { get; set; } = 0.2;
        public int max_tokens { get; set; } = 1000;
    }

    /// <summary>
    /// PerplexityAI message model
    /// </summary>
    public class PerplexityMessage
    {
        public string role { get; set; } = string.Empty; // "system", "user", "assistant"
        public string content { get; set; } = string.Empty;
    }

    /// <summary>
    /// PerplexityAI API response model
    /// </summary>
    public class PerplexityResponse
    {
        public string id { get; set; } = string.Empty;
        public string @object { get; set; } = string.Empty;
        public long created { get; set; }
        public string model { get; set; } = string.Empty;
        public List<PerplexityChoice> choices { get; set; } = new();
        public PerplexityUsage usage { get; set; } = new();
    }

    /// <summary>
    /// PerplexityAI choice model
    /// </summary>
    public class PerplexityChoice
    {
        public int index { get; set; }
        public string finish_reason { get; set; } = string.Empty;
        public PerplexityMessage message { get; set; } = new();
    }

    /// <summary>
    /// PerplexityAI usage model
    /// </summary>
    public class PerplexityUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
