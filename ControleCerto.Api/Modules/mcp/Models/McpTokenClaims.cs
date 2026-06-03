namespace ControleCerto.Modules.Mcp.Models
{
    public class McpTokenClaims
    {
        public int UserId { get; init; }

        public string TokenType { get; init; } = string.Empty;

        public DateTime IssuedAtUtc { get; init; }

        public DateTime ExpiresAtUtc { get; init; }

        public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
    }
}
