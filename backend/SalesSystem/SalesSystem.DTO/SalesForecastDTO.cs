using System.Collections.Generic;

namespace SalesSystem.DTO
{
    public class SalesForecastDTO
    {
        /// <summary>False when there are fewer complete months than the model needs; Forecast is then empty.</summary>
        public bool HasEnoughData { get; set; }
        public string? Message { get; set; }
        public string Method { get; set; } = "Holt linear trend (double exponential smoothing)";

        /// <summary>Complete months used to fit the model, oldest first.</summary>
        public List<RevenuePointDTO> History { get; set; } = new();

        /// <summary>Revenue registered so far in the current (incomplete) month.</summary>
        public RevenuePointDTO? CurrentMonth { get; set; }

        /// <summary>Forecast starting with the current month, with a ~95% interval.</summary>
        public List<RevenuePointDTO> Forecast { get; set; } = new();
    }

    public class RevenuePointDTO
    {
        /// <summary>yyyy-MM</summary>
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int? Sales { get; set; }
        public decimal? Lower { get; set; }
        public decimal? Upper { get; set; }
    }
}
