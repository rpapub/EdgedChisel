using System;
using System.Collections.Generic;

namespace Cprima.RpaPub.EdgedChisel
{
    public enum StatusErrorType
    {
        Retryable,
        NonRetryable,
        Unknown
    }

    public static class OutputStatusBuilder
    {
        public static Dictionary<string, object> CreateStatusObject(
            bool isSuccess = false,
            string errorMessage = null,
            string errorType = null,
            string code = null,
            Dictionary<string, object> details = null,
            string retryAfter = null,
            string source = null,
            string exceptionType = null,
            int durationMs = 0)
        {
            if (!string.IsNullOrEmpty(errorType) &&
                !Enum.TryParse(typeof(StatusErrorType), errorType, ignoreCase: true, out _))
            {
                throw new ArgumentException($"Invalid errorType: {errorType}");
            }

            return new Dictionary<string, object>
            {
                { "isSuccess", isSuccess },
                { "errorMessage", errorMessage },
                { "errorType", errorType },
                { "code", code },
                { "details", details ?? new Dictionary<string, object>() },
                { "retryAfter", retryAfter },
                { "source", source },
                { "exceptionType", exceptionType },
                { "timestamp", DateTime.UtcNow.ToString("o") },
                { "durationMs", durationMs }
            };
        }

        // Success setter
        public static Dictionary<string, object> SetSuccess(
            string code = null,
            Dictionary<string, object> details = null,
            int durationMs = 0)
        {
            return CreateStatusObject(
                isSuccess: true,
                code: code,
                details: details,
                durationMs: durationMs
            );
        }

        // Exception-based setters
        public static Dictionary<string, object> SetRetryable(Exception ex) =>
            SetRetryable(
                errorMessage: ex?.Message,
                exceptionType: ex?.GetType().ToString()
            );

        public static Dictionary<string, object> SetNonRetryable(Exception ex) =>
            SetNonRetryable(
                errorMessage: ex?.Message,
                exceptionType: ex?.GetType().ToString()
            );

        // Manual setters
        public static Dictionary<string, object> SetRetryable(
            string errorMessage = null,
            string exceptionType = null)
        {
            return CreateStatusObject(
                isSuccess: false,
                errorMessage: errorMessage,
                errorType: StatusErrorType.Retryable.ToString().ToLower(),
                exceptionType: exceptionType,
                retryAfter: DateTime.UtcNow.AddSeconds(5).ToString("o")
            );
        }

        public static Dictionary<string, object> SetNonRetryable(
            string errorMessage = null,
            string exceptionType = null)
        {
            return CreateStatusObject(
                isSuccess: false,
                errorMessage: errorMessage,
                errorType: StatusErrorType.NonRetryable.ToString().ToLower(),
                exceptionType: exceptionType
            );
        }
    }
}