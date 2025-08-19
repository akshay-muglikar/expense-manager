namespace InventoryManagement.Api.AI.Models
{
    /// <summary>
    /// Enum defining different skill types
    /// </summary>
    public enum SkillType
    {
        Inventory,
        Billing,
        Trends,
        SqlAnalysis,
        General
    }

    /// <summary>
    /// Request model for skill execution
    /// </summary>
    public class SkillRequest
    {
        public string UserQuery { get; set; } = string.Empty;
        public string? ClientId { get; set; }
        public SkillType SkillType { get; set; }
    }

    /// <summary>
    /// Response model from skill execution
    /// </summary>
    public class SkillResponse
    {
        public string Response { get; set; } = string.Empty;
        public string DataSummary { get; set; } = string.Empty;
        public bool UsedLocalData { get; set; }
        public SkillType SkillType { get; set; }
        public List<string> SuggestedQuestions { get; set; } = new();
    }
}
