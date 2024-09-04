using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IDataPrediction
    {
        Task TrainAndSaveModel(string modelPathTemp);
        Task<SalePredictionDTO> PredictSale(SaleToPredictDTO saleToPredictDTO);

        Task RunModelOperations();
    }
}
