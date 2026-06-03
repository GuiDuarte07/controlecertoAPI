using System.Text.Json;
using System.Text.Json.Nodes;
using ControleCerto.Modules.Mcp.Decorators;
using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Mcp.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("mcp")]
    [McpAuthorize]
    public class McpProtocolController : ControllerBase
    {
        private const string JsonRpcVersion = "2.0";
        private const string ProtocolVersion = "2025-03-26";

        private readonly IToolRegistryService _toolRegistryService;
        private readonly IMcpCommandService _mcpCommandService;
        private readonly ILogger<McpProtocolController> _logger;

        public McpProtocolController(
            IToolRegistryService toolRegistryService,
            IMcpCommandService mcpCommandService,
            ILogger<McpProtocolController> logger)
        {
            _toolRegistryService = toolRegistryService;
            _mcpCommandService = mcpCommandService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleAsync([FromBody] JsonElement body, CancellationToken cancellationToken)
        {
            var tokenClaims = HttpContext.Items[McpHttpContextKeys.Claims] as McpTokenClaims;
            if (tokenClaims is null)
            {
                return Unauthorized(McpCommandResponse.FromError("authorization", "Token MCP inválido."));
            }

            if (body.ValueKind == JsonValueKind.Array)
            {
                var batchResponses = new JsonArray();

                foreach (var request in body.EnumerateArray())
                {
                    var response = await HandleSingleAsync(request, tokenClaims, cancellationToken);
                    if (response is not null)
                    {
                        batchResponses.Add(response);
                    }
                }

                if (batchResponses.Count == 0)
                {
                    return NoContent();
                }

                return new JsonResult(batchResponses);
            }

            if (body.ValueKind != JsonValueKind.Object)
            {
                return new JsonResult(CreateErrorResponse(
                    id: null,
                    code: -32600,
                    message: "Invalid Request."));
            }

            var singleResponse = await HandleSingleAsync(body, tokenClaims, cancellationToken);
            if (singleResponse is null)
            {
                return NoContent();
            }

            return new JsonResult(singleResponse);
        }

        [HttpGet]
        public IActionResult GetInfo()
        {
            return Ok(new
            {
                name = "controlecerto-mcp",
                protocolVersion = ProtocolVersion,
                transport = "streamable-http"
            });
        }

        private async Task<JsonObject?> HandleSingleAsync(
            JsonElement request,
            McpTokenClaims tokenClaims,
            CancellationToken cancellationToken)
        {
            if (request.ValueKind != JsonValueKind.Object)
            {
                return CreateErrorResponse(id: null, code: -32600, message: "Invalid Request.");
            }

            if (!request.TryGetProperty("method", out var methodElement) || methodElement.ValueKind != JsonValueKind.String)
            {
                return CreateErrorResponse(GetIdNode(request), -32600, "Method is required.");
            }

            var method = methodElement.GetString() ?? string.Empty;
            var hasId = request.TryGetProperty("id", out var idElement);
            var idNode = hasId ? JsonNode.Parse(idElement.GetRawText()) : null;
            var hasParams = request.TryGetProperty("params", out var paramsElement);

            if (!hasId && method.StartsWith("notifications/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            switch (method)
            {
                case "initialize":
                    return CreateSuccessResponse(idNode, BuildInitializeResult(paramsElement));

                case "ping":
                    return CreateSuccessResponse(idNode, new { });

                case "notifications/initialized":
                    return null;

                case "tools/list":
                    return CreateSuccessResponse(idNode, BuildToolsListResult(tokenClaims));

                case "tools/call":
                    if (!hasParams || paramsElement.ValueKind != JsonValueKind.Object)
                    {
                        return CreateErrorResponse(idNode, -32602, "Invalid params for tools/call.");
                    }

                    return await ExecuteToolCallAsync(paramsElement, tokenClaims, idNode, cancellationToken);

                default:
                    _logger.LogDebug("Método MCP não suportado recebido: {Method}", method);
                    return CreateErrorResponse(idNode, -32601, $"Method not found: {method}");
            }
        }

        private object BuildInitializeResult(JsonElement paramsElement)
        {
            var protocolVersion = ProtocolVersion;

            if (paramsElement.ValueKind == JsonValueKind.Object
                && paramsElement.TryGetProperty("protocolVersion", out var requestedVersion)
                && requestedVersion.ValueKind == JsonValueKind.String)
            {
                protocolVersion = requestedVersion.GetString() ?? ProtocolVersion;
            }

            return new
            {
                protocolVersion,
                capabilities = new
                {
                    tools = new
                    {
                        listChanged = false
                    }
                },
                serverInfo = new
                {
                    name = "controlecerto-mcp",
                    version = "1.0.0"
                }
            };
        }

        private object BuildToolsListResult(McpTokenClaims tokenClaims)
        {
            var tools = _toolRegistryService
                .GetToolsForPermissions(tokenClaims.Permissions)
                .Select(tool => new
                {
                    name = tool.Name,
                    description = tool.Description,
                    inputSchema = tool.Parameters
                })
                .ToArray();

            return new
            {
                tools
            };
        }

        private async Task<JsonObject> ExecuteToolCallAsync(
            JsonElement paramsElement,
            McpTokenClaims tokenClaims,
            JsonNode? idNode,
            CancellationToken cancellationToken)
        {
            if (!paramsElement.TryGetProperty("name", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
            {
                return CreateErrorResponse(idNode, -32602, "Tool name is required.");
            }

            var toolName = nameElement.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return CreateErrorResponse(idNode, -32602, "Tool name is required.");
            }

            var visibleTools = _toolRegistryService.GetToolsForPermissions(tokenClaims.Permissions);
            var tool = visibleTools.FirstOrDefault(item => string.Equals(item.Name, toolName, StringComparison.OrdinalIgnoreCase));

            if (tool is null)
            {
                return CreateSuccessResponse(idNode, new
                {
                    content = new[]
                    {
                        new { type = "text", text = $"Tool '{toolName}' não está disponível para este token MCP." }
                    },
                    isError = true
                });
            }

            JsonElement? payload = null;
            if (paramsElement.TryGetProperty("arguments", out var argumentsElement)
                && argumentsElement.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                if (argumentsElement.ValueKind != JsonValueKind.Object)
                {
                    return CreateErrorResponse(idNode, -32602, "Tool arguments must be a JSON object.");
                }

                payload = argumentsElement;
            }

            var commandResponse = await _mcpCommandService.ExecuteAsync(
                new McpCommandRequest
                {
                    Resource = tool.Resource,
                    Action = tool.Action,
                    Payload = payload
                },
                tokenClaims,
                cancellationToken);

            if (commandResponse.Success)
            {
                var structuredContent = NormalizeStructuredContent(commandResponse.Data);
                var contentText = SerializeForToolContent(commandResponse.Data);

                return CreateSuccessResponse(idNode, new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = contentText
                        }
                    },
                    structuredContent,
                    isError = false
                });
            }

            var errorText = string.Join("; ", commandResponse.Errors.Select(error => $"{error.Field}: {error.Message}"));

            return CreateSuccessResponse(idNode, new
            {
                content = new[]
                {
                    new { type = "text", text = string.IsNullOrWhiteSpace(errorText) ? "Falha ao executar tool." : errorText }
                },
                structuredContent = new
                {
                    success = false,
                    errors = commandResponse.Errors
                },
                isError = true
            });
        }

        private static JsonObject CreateSuccessResponse(JsonNode? idNode, object result)
        {
            return new JsonObject
            {
                ["jsonrpc"] = JsonRpcVersion,
                ["id"] = idNode,
                ["result"] = JsonSerializer.SerializeToNode(result)
            };
        }

        private static JsonObject CreateErrorResponse(JsonNode? id, int code, string message)
        {
            return new JsonObject
            {
                ["jsonrpc"] = JsonRpcVersion,
                ["id"] = id,
                ["error"] = new JsonObject
                {
                    ["code"] = code,
                    ["message"] = message
                }
            };
        }

        private static object NormalizeStructuredContent(object? data)
        {
            if (data is null)
            {
                return new { };
            }

            var jsonElement = JsonSerializer.SerializeToElement(data);

            return jsonElement.ValueKind == JsonValueKind.Object
                ? data
                : new { data };
        }

        private static string SerializeForToolContent(object? data)
        {
            if (data is null)
            {
                return "{}";
            }

            return JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions
                {
                    WriteIndented = false
                });
        }

        private static JsonNode? GetIdNode(JsonElement request)
        {
            if (!request.TryGetProperty("id", out var idElement))
            {
                return null;
            }

            return JsonNode.Parse(idElement.GetRawText());
        }
    }
}
