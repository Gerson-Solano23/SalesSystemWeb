using SalesSystem.BLL.Services.Contract;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SalesSystem.Utility;

namespace SalesSystem.BLL.Services
{
    /// <summary>
    /// Uploads the Excel report of the last complete week (Monday to Sunday) to S3.
    /// It checks periodically and only generates the report when it does not exist yet,
    /// so restarts never skip or duplicate a week, and a failure is logged and retried
    /// instead of stopping the whole API.
    /// </summary>
    public class WeeklyTaskService : BackgroundService, IWeeklyTask
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        private readonly ILogger<WeeklyTaskService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly bool _enabled;

        public WeeklyTaskService(ILogger<WeeklyTaskService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _enabled = !bool.TryParse(configuration["WeeklyReport:Enabled"], out bool enabled) || enabled;
        }

        public async Task<bool> ExecuteWeeklyTask()
        {
            var (start, end, fileName) = GetLastCompletedWeek(DateTime.Now);

            using var scope = _scopeFactory.CreateScope();
            var uploadFile = scope.ServiceProvider.GetRequiredService<IUploadS3File>();
            var sale = scope.ServiceProvider.GetRequiredService<ISale>();
            var fileDownload = scope.ServiceProvider.GetRequiredService<IFileDownload>();

            if (await uploadFile.FileExists(fileName))
            {
                return true;
            }

            var listReports = await sale.Report(start.ToString(DateParser.Format, CultureInfo.InvariantCulture),
                                                end.ToString(DateParser.Format, CultureInfo.InvariantCulture));
            if (listReports.Count == 0)
            {
                return true;
            }

            _logger.LogInformation("Uploading weekly report {FileName} ({Start:yyyy-MM-dd} - {End:yyyy-MM-dd})", fileName, start, end);
            var file = await fileDownload.GetExcelFile(listReports);
            return await uploadFile.UploadFile(file, fileName, ExcelContentType);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _logger.LogInformation("WeeklyTaskService is disabled (WeeklyReport:Enabled = false).");
                return;
            }

            _logger.LogInformation("WeeklyTaskService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteWeeklyTask();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing weekly task, it will be retried in {Interval}", CheckInterval);
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Monday-to-Sunday week before the one containing <paramref name="now"/>, and its report file name.
        /// </summary>
        public static (DateTime Start, DateTime End, string FileName) GetLastCompletedWeek(DateTime now)
        {
            DateTime today = now.Date;
            int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            DateTime start = today.AddDays(-daysSinceMonday - 7);
            DateTime end = start.AddDays(6);
            string fileName = $"Report_Week-{ISOWeek.GetYear(start)}-W{ISOWeek.GetWeekOfYear(start):00}.xlsx";
            return (start, end, fileName);
        }
    }
}
