using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IFileDownload
    {
        Task DownloadExcel(int idUser);
        Task<byte[]> getFileExcel(int idUser);

        Task runTaskFileExcel(int idUser);

        bool DeleteFile(int idUser);

    }
}
