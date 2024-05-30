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
        }
    }
}
