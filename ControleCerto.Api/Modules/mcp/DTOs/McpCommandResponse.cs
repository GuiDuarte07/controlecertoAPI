namespace ControleCerto.Modules.Mcp.DTOs
{
    public class McpCommandResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public List<McpCommandError> Errors { get; set; } = new();

        public static McpCommandResponse FromSuccess(object? data)
        {
            return new McpCommandResponse
            {
                Success = true,
                Data = data
            };
        }

        public static McpCommandResponse FromError(string field, string message)
        {
            return new McpCommandResponse
            {
                Success = false,
                Errors = new List<McpCommandError>
                {
                    new() { Field = field, Message = message }
                }
            };
        }

        public static McpCommandResponse FromErrors(IEnumerable<McpCommandError> errors)
        {
            return new McpCommandResponse
            {
                Success = false,
                Errors = errors.ToList()
            };
        }
    }

    public class McpCommandError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
