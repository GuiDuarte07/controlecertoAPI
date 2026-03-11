using ControleCerto.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult HandleReturnResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Value);
            }

            var errorPayload = ErrorResponse.FromAppError(result.Error);

            return new ObjectResult(errorPayload)
            {
                StatusCode = errorPayload.Code
            };
        }
    }
}
