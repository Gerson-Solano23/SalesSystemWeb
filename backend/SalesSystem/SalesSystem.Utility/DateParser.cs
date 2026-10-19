using System;
using System.Globalization;

namespace SalesSystem.Utility
{
    /// <summary>
    /// Single place to parse the dd/MM/yyyy dates the frontend sends, so every endpoint
    /// accepts the same format and answers a bad date with a clear 400 instead of a 500.
    /// </summary>
    public static class DateParser
    {
        public const string Format = "dd/MM/yyyy";
        private static readonly string[] AcceptedFormats = { "dd/MM/yyyy", "d/M/yyyy" };

        public static DateTime Parse(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value)
                || !DateTime.TryParseExact(value.Trim(), AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                throw new BusinessException($"Invalid {fieldName}, expected format {Format}");
            }
            return date;
        }

        public static (DateTime Start, DateTime End) ParseRange(string? start, string? end)
        {
            DateTime startDate = Parse(start, "start date");
            DateTime endDate = Parse(end, "end date");
            if (startDate > endDate)
            {
                throw new BusinessException("The start date must be before the end date");
            }
            return (startDate.Date, endDate.Date);
        }
    }
}
