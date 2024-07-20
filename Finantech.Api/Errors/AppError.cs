using Finantech.Enums;

namespace Finantech.Errors
{
    public class AppError
    {
        public string ErrorMessage { get; set; }
        public ErrorTypeEnum ErrorType { get; set; }

        public AppError(string errorMessage, ErrorTypeEnum errorType) 
        {
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }
    }
}
