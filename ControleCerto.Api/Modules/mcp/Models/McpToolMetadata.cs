using System.Text.Json.Nodes;

namespace ControleCerto.Modules.Mcp.Models
{
    public class McpToolMetadata
    {
        public string Name { get; init; } = string.Empty;
        public string Resource { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public JsonNode Parameters { get; init; } = new JsonObject();
        public IReadOnlyCollection<string> RequiredPermissions { get; init; } = Array.Empty<string>();
    }
}
