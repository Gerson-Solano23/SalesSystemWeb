using SalesSystem.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IFileDownload
    {
        /// <summary>Builds the Excel report for the given rows. Stateless: nothing is kept between requests.</summary>
        Task<byte[]> GetExcelFile(List<ReportDTO> list);
    }
}
