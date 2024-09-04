using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NPOI.SS.Formula.Functions;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class DataPredictionService : IDataPrediction
    {
        private readonly MLContext mlcontext;
        private readonly DBSalesContext _dbContext;
        private readonly ISalesRepository _saleRepository;
     
        
        public DataPredictionService(MLContext mlcontext, DBSalesContext dbContext, ISalesRepository saleRepository)
        {
            this.mlcontext = mlcontext;
            _dbContext = dbContext;
            _saleRepository = saleRepository;
        }

        public async Task TrainAndSaveModel(string modelPathTemp)
        {
            // Verificar si el modelo ya existe
            if (ModelExists(modelPathTemp))
            {
                Console.WriteLine("El modelo ya existe. Cargando el modelo existente.");
                return;
            }
            IQueryable<Sale> query = await _saleRepository.Consult();
            var ResultList = new List<Sale>();
            try
            {
                ResultList = await query.ToListAsync();

                //list = await _dbContext.Sales.ToListAsync();

                var baseDate = new DateTime(1970, 1, 1); // Fecha base para convertir

                var transformedSalesList = ResultList.Select(s => new SaleToPredictDTO
                {
                    PaymentType = s.PaymentType,
                    Total = s.Total.HasValue ? (float)s.Total.Value : 0.0f,
                    DateRegistryAsNumber = (float)(s.DateRegistry - baseDate).Value.TotalDays
                }).ToList();

                // Paso 1: Cargar datos
                IDataView dataView = mlcontext.Data.LoadFromEnumerable(transformedSalesList);

                // Definir pipeline
                var pipeline = mlcontext.Transforms.Conversion.MapValueToKey("PaymentType", nameof(SaleToPredictDTO.PaymentType))
                    .Append(mlcontext.Transforms.Categorical.OneHotEncoding("PaymentTypeEncoded", "PaymentType"))
                    .Append(mlcontext.Transforms.Concatenate("Features", "PaymentTypeEncoded", "DateRegistryAsNumber"))
                    .Append(mlcontext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(SaleToPredictDTO.Total))) // Mapea "Total" a "Label"
                    .Append(mlcontext.Regression.Trainers.Sdca(labelColumnName: "Label"));

                // Entrenar modelo
                var model = pipeline.Fit(dataView);

                // Evaluar modelo
                var predictions = model.Transform(dataView);
                var metrics = mlcontext.Regression.Evaluate(predictions, labelColumnName: "Label");

                // Guardar el modelo
                using (var stream = new FileStream(modelPathTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    mlcontext.Model.Save(model, dataView.Schema, stream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<SalePredictionDTO> PredictSale(SaleToPredictDTO saleToPredict)
        {
            // Cargar el modelo
            ITransformer loadedModel;
            string modelPath = "model.zip";
            // Entrenar y guardar el modelo si no existe
            await TrainAndSaveModel(modelPath);
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlcontext.Model.Load(stream, out var modelInputSchema);
            }

            // Crear un predictor usando el modelo cargado
            var predictionEngine = mlcontext.Model.CreatePredictionEngine<SaleToPredictDTO, SalePredictionDTO>(loadedModel);

            // Usar el predictor para hacer predicciones
            var prediction = predictionEngine.Predict(saleToPredict);

            return prediction;
        }

        public bool ModelExists(string modelPath)
        {
            return File.Exists(modelPath);
        }
        public ITransformer LoadModel(string modelPath)
        {
            if (!ModelExists(modelPath))
            {
                throw new FileNotFoundException("El archivo del modelo no se encuentra.", modelPath);
            }

            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlcontext.Model.Load(stream, out var modelInputSchema);
            }

            return loadedModel;
        }
        public async Task RunModelOperations()
        {
            string modelPath = "model.zip";

            // Entrenar y guardar el modelo si no existe
            await TrainAndSaveModel(modelPath);

            // Cargar el modelo para hacer predicciones
            ITransformer model = LoadModel(modelPath);

            var predictionEngine = mlcontext.Model.CreatePredictionEngine<SaleToPredictDTO, SalePredictionDTO>(model);
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime targetDate = new DateTime(2024, 9, 30);
            float dateRegistryAsNumber = (float)(targetDate - baseDate).TotalDays;
            var saleToPredict = new SaleToPredictDTO
            {
                PaymentType = "CreditCard",
                DateRegistryAsNumber = dateRegistryAsNumber
            };

            var prediction = predictionEngine.Predict(saleToPredict);

            Console.WriteLine($"Predicted Total Sale: {prediction.Score}");
        }

    }
}
