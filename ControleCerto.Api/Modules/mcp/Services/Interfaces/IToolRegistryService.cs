using ControleCerto.Modules.Mcp.Models;

namespace ControleCerto.Modules.Mcp.Services.Interfaces
{
    public interface IToolRegistryService
    {
        IReadOnlyCollection<McpToolMetadata> GetAllTools();
        IReadOnlyCollection<McpToolMetadata> GetToolsForPermissions(IEnumerable<string> permissions);
        McpToolMetadata? GetByResourceAction(string resource, string action);
        McpToolMetadata? GetByName(string name);
    }
}
