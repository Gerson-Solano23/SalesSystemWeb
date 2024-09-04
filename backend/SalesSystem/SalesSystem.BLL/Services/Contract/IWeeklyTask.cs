using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IWeeklyTask
    {
        Task<bool> ExecuteWeeklyTask();
    }
}
