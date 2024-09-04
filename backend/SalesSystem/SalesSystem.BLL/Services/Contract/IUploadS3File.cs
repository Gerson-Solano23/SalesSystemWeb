using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IUploadS3File
    {
        Task<bool> UploadFile(byte[] file, string fileName, string fileContentType);

        Task<bool> DeleteFile(string fileName);

        Task<byte[]> DownloadFile(string fileName);

        Task<List<string>> getListFilesPerMonth(string month, string year);  
    }
}
