using SalesSystem.DTO;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IDataPrediction
    {
        /// <summary>Forecasts the revenue of the current month and the following ones.</summary>
        Task<SalesForecastDTO> ForecastMonthlyRevenue(int months);
    }
}
