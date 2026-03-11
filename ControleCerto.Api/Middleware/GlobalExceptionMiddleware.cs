using ControleCerto.Enums;
using ControleCerto.Errors;

namespace ControleCerto.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);

                int statusCode;
                AppError appError;

                if (IsPayloadTooLarge(exception))
                {
                    statusCode = StatusCodes.Status413PayloadTooLarge;
                    appError = new AppError("O arquivo deve ter no máximo 10MB.", ErrorTypeEnum.Validation);
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    appError = new AppError(
                        "Ocorreu um erro interno. Tente novamente mais tarde.",
                        ErrorTypeEnum.InternalError);
                }

                var payload = ErrorResponse.FromAppError(appError, statusCode);

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(payload);
            }
        }

        private static bool IsPayloadTooLarge(Exception exception)
        {
            if (exception is BadHttpRequestException badRequestException &&
                badRequestException.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                return true;
            }

            if (exception is InvalidDataException invalidDataException &&
                invalidDataException.Message.Contains("Multipart body length limit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (exception is IOException ioException &&
                ioException.Message.Contains("request body too large", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return exception.InnerException is not null && IsPayloadTooLarge(exception.InnerException);
        }
    }
}