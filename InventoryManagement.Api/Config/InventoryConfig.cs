
namespace InventoryManagement.Api.Config
{
    public class InventoryConfig
    {
        public WhatsAppConfig WhatsAppMessage { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public string AppUrl { get; set; }
        public ConnectionSetting ConnectionStrings { get; set; }
        public PerplexityConfig PerplexityAI { get; set; }
    }
    public class PerplexityConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.perplexity.ai/chat/completions";
        public string Model { get; set; } = "llama-3.1-sonar-small-128k-online";
        public double Temperature { get; set; } = 0.2;
        public int MaxTokens { get; set; } = 1000;
    }
    public class WhatsAppConfig
    {
        public string Token { get; set; }
        public string PhoneId { get; set; }
        public string WhatsBusinessId { get; set; }
    }
    public class JwtSettings
    {
        public string UrlKey { get; set; }
    }
    public class ConnectionSetting
    {
        public string Database { get; set; }
        public string UserDatabase { get; set; }
    }
}