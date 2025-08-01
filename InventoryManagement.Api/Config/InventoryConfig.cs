
namespace InventoryManagement.Api.Config
{
    public class InventoryConfig
    {
        public WhatsAppConfig WhatsAppMessage { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public string AppUrl { get; set; }
    }

    public class WhatsAppConfig
    {
        public string Token {get; set;}
        public string PhoneId {get; set;}
        public string WhatsBusinessId {get; set;}
    }
    public class JwtSettings
    {
        public string UrlKey { get; set; }
    }
}