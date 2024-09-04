using SalesSystem.BLL.Services.Contract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SalesSystem.Entity;

namespace SalesSystem.BLL.Services
{
    public class WeeklyTaskService : BackgroundService, IWeeklyTask
    {
        private readonly ILogger<WeeklyTaskService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public WeeklyTaskService(ILogger<WeeklyTaskService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<bool> ExecuteWeeklyTask()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var uploadFile = scope.ServiceProvider.GetRequiredService<IUploadS3File>();
                    var sale = scope.ServiceProvider.GetRequiredService<ISale>();
                    var fileDownload = scope.ServiceProvider.GetRequiredService<IFileDownload>();

                    DateTime now = DateTime.Now;
                    CultureInfo ci = CultureInfo.CurrentCulture;
                    Calendar calendar = ci.Calendar;

                    DayOfWeek firstDayOfWeek = ci.DateTimeFormat.FirstDayOfWeek;
                    CalendarWeekRule weekRule = ci.DateTimeFormat.CalendarWeekRule;

                    int weekOfYear = calendar.GetWeekOfYear(now, weekRule, firstDayOfWeek);

                    var firstDateOfWeek = FirstDateOfWeek(now.Year, weekOfYear);
                    var lastDateOfWeek = GetLastDayOfWeek(now.Year, weekOfYear);

                    var listReports = await sale.Report(firstDateOfWeek.ToString("yyyy-MM-dd"), lastDateOfWeek.ToString("yyyy-MM-dd"));

                    if (listReports.Count > 0)
                    {
                        var file = await fileDownload.GetExcelFile(listReports);
                        if (file != null)
                        {
                            byte[] fileBytes = file;
                            string fileName = "myfile.xlsx";
                            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                         

                            bool response = await uploadFile.UploadFile(fileBytes, fileName, contentType);
                            return response;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing weekly task");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WeeklyTaskService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                CultureInfo ci = new CultureInfo("es-CR");
                Calendar calendar = ci.Calendar;
                CalendarWeekRule weekRule = ci.DateTimeFormat.CalendarWeekRule;
                DayOfWeek firstDayOfWeek = ci.DateTimeFormat.FirstDayOfWeek;

                int weekOfYear = calendar.GetWeekOfYear(now, weekRule, firstDayOfWeek);
                var dayOfWeek = GetLastDayOfWeek(now.Year, weekOfYear);

                if (now.DayOfWeek == dayOfWeek.DayOfWeek)
                {
                    _logger.LogInformation($"Ejecutando tarea semanal el día {now:yyyy-MM-dd}");

                    // Ejecuta tu tarea aquí
                    var result = await ExecuteWeeklyTask();

                    // Espera hasta el próximo domingo (7 días)
                    await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                }
                else
                {
                    // Espera hasta el siguiente día para verificar
                    await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                }
            }

            _logger.LogInformation("WeeklyTaskService is stopping.");
        }

        public static DateTime GetLastDayOfWeek(int year, int weekNumber)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Sunday - jan1.DayOfWeek;

            DateTime firstSunday = jan1.AddDays(daysOffset);
            DateTime targetWeekStart = firstSunday.AddDays((weekNumber - 1) * 7);

            return targetWeekStart.AddDays(7);
        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            Calendar cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }
}
