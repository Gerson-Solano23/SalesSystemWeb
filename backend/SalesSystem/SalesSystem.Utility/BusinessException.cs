using System;

namespace SalesSystem.Utility
{
    /// <summary>
    /// Expected business-rule failure. Its message is safe to show to the user;
    /// any other exception is logged and answered with a generic message.
    /// </summary>
    public class BusinessException : Exception
    {
        public int StatusCode { get; }

        public BusinessException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }

        public static BusinessException NotFound(string message) => new BusinessException(message, 404);
    }
}
