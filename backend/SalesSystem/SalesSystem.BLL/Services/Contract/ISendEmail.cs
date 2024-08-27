using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface ISendEmail
    {
        Task<bool> SendEmail(EmailDTO emailDTO);

        Task<byte[]> createPDFFile(EmailDTO emailDTO);
    }
}
