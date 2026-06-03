using System.Text.Json.Nodes;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class ToolMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JsonNode Parameters { get; set; } = new JsonObject();
        public IReadOnlyCollection<string> RequiredPermissions { get; set; } = Array.Empty<string>();
    }
}
