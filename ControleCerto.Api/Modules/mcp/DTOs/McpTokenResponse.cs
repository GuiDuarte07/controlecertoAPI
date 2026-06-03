namespace ControleCerto.Modules.Mcp.DTOs
{
    public class McpTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
