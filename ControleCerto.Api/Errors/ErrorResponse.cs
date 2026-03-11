using System;
using System.Collections.Generic;
using System.Linq;
using ControleCerto.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ControleCerto.Errors
{
    public class ErrorResponse
    {
        public int Code { get; init; }
        public string Detail { get; init; } = string.Empty;
        public string ErrorType { get; init; } = string.Empty;
        public DateTime TimestampUtc { get; init; }
        public IDictionary<string, string[]>? Errors { get; init; }

        public static ErrorResponse FromAppError(AppError appError, int? statusCodeOverride = null)
        {
            var statusCode = statusCodeOverride ?? GetStatusCode(appError.ErrorType);

            return new ErrorResponse
            {
                Code = statusCode,
                Detail = appError.ErrorMessage,
                ErrorType = appError.ErrorType.ToString(),
                TimestampUtc = DateTime.UtcNow
            };
        }

        public static ErrorResponse FromModelState(ModelStateDictionary modelState, int statusCodeOverride = StatusCodes.Status400BadRequest)
        {
            var errors = modelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value." : error.ErrorMessage)
                        .ToArray());

            return new ErrorResponse
            {
                Code = statusCodeOverride,
                Detail = "One or more validation errors occurred.",
                ErrorType = ErrorTypeEnum.Validation.ToString(),
                TimestampUtc = DateTime.UtcNow,
                Errors = errors.Count == 0 ? null : errors
            };
        }

        private static int GetStatusCode(ErrorTypeEnum errorType)
        {
            return errorType switch
            {
                ErrorTypeEnum.BusinessRule => StatusCodes.Status400BadRequest,
                ErrorTypeEnum.Validation => StatusCodes.Status422UnprocessableEntity,
                ErrorTypeEnum.NotFound => StatusCodes.Status404NotFound,
                ErrorTypeEnum.Conflict => StatusCodes.Status400BadRequest,
                ErrorTypeEnum.NotImplemented => StatusCodes.Status501NotImplemented,
                ErrorTypeEnum.InternalError => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError,
            };
        }
    }
}