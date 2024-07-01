using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface ISale
    {
        Task<SaleDTO> Create(SaleDTO entity);

        Task<List<SaleDTO>> History(string searchBy, string saleNumber, string Startdate, string endDate);

        Task<List<ReportDTO>> Report(string startDate, string endDate);

    }
}