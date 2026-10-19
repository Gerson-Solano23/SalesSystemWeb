using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesSystem.DAL.DbSalesContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DAL.Repositories;
using SalesSystem.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.BLL.Services;
using SalesSystem.DTO;
using Microsoft.ML;

namespace SalesSystem.IOC
{
    public static class Dependecy
    {
        public static void InjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DBSalesContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("StringConnection"));
            });

            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISalesRepository, SalesRepository>();

            services.AddAutoMapper(typeof(AutoMapperProfile));

            services.AddScoped<IRol, RolService>();
            services.AddScoped<IUsuario, UsuarioService>();
            services.AddScoped<ICategory, CategoryService>();
            services.AddScoped<IProduct, ProductService>();
            services.AddScoped<ISale, SaleService>();
            services.AddScoped<IDashBoard, DashBoardService>();
            services.AddScoped<IMenu, MenuService>();
            services.AddScoped<IFileDownload, FileDownloadService>();
            services.AddSingleton(new EmailSettings
            {
                Host = configuration["Email:Host"] ?? "smtp-mail.outlook.com",
                Port = int.TryParse(configuration["Email:Port"], out int port) ? port : 587,
                EnableSsl = !bool.TryParse(configuration["Email:EnableSsl"], out bool ssl) || ssl,
                User = configuration["Email:User"] ?? string.Empty,
                Password = configuration["Email:Password"] ?? Environment.GetEnvironmentVariable("PASSGREATEINDUSTRIES") ?? string.Empty,
                NotificationEmail = configuration["Email:NotificationEmail"] ?? Environment.GetEnvironmentVariable("MYOWNEMAIL") ?? string.Empty
            });
            services.AddScoped<ISendEmail, EmailSenderService>();
            // services.AddSingleton<IUploadS3File, UploadFileService>();
            services.AddScoped<IUploadS3File, UploadFileService>();

            services.AddScoped<IDataPrediction, DataPredictionService>();
         
            services.AddSingleton<MLContext>(sp => new MLContext());
            //services.AddScoped<IFormFile, UploadFileService>();
            services.AddSingleton<IWeeklyTask, WeeklyTaskService>();
            services.AddHostedService<WeeklyTaskService>();

        }
    }
}
