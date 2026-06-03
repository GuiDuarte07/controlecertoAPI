using System.Text.Json;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class McpCommandRequest
    {
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public JsonElement? Payload { get; set; }
    }
}
