using Finantech.Enums;
using Finantech.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult HandleError<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Value);
            }

            return result.Error.ErrorType switch
            {
                ErrorTypeEnum.BusinessRule => new BadRequestObjectResult(result.Error.ErrorMessage),
                ErrorTypeEnum.Validation => new UnprocessableEntityObjectResult(result.Error.ErrorMessage),
                ErrorTypeEnum.InternalError => new StatusCodeResult(StatusCodes.Status500InternalServerError),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
            };
        }
    }
}
