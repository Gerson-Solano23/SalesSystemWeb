using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using NPOI.SS.Util;
using System.Diagnostics;

namespace SalesSystem.BLL.Services
{
    public class FileDownloadService : IFileDownload
    {
        private static byte[] fileExcel;
        private static Dictionary<int, byte[]> files = new Dictionary<int, byte[]>();
        private static Dictionary<int, bool> isRunning = new Dictionary<int, bool>();
        private byte[] pictureData;
        private readonly ISale _saleService;


        public FileDownloadService(ISale saleService)
        {
            _saleService = saleService;
        }


        public async Task DownloadExcel(int idUser)
        {

            if (isRunning.ContainsKey(idUser))
            {
                isRunning[idUser] = true;
            }
            else
            {
                isRunning.Add(idUser, true);
            }


            List<ReportDTO> listReports = new List<ReportDTO>();
            listReports = _saleService.getList();

            //TASKS
            var productAsync = getMostSale(listReports);
            var pymentTypeMostUsed = getMostPymentTypeUsed(listReports);
            var range = GetDateRange(listReports);
            var totakSales = getTotalSales(listReports);

            var workBook = new XSSFWorkbook();

            var sheet = workBook.CreateSheet("Report");

            //CONFIG IMG LOGO
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var relativePath = "PicturesAndFiles" + Path.DirectorySeparatorChar + "logo.png";//GET IMAGE FROM PATH
            var filePath = Path.Combine(basePath, relativePath);
            pictureData = System.IO.File.ReadAllBytes(filePath);
            var pictureIndex = workBook.AddPicture(pictureData, NPOI.SS.UserModel.PictureType.PNG);//put the format
            var patriarch = sheet.CreateDrawingPatriarch();//ADD PICTURE TO WORKBOOK
            IClientAnchor anchor = workBook.GetCreationHelper().CreateClientAnchor();
            // Establecer el ancla de la imagen para que se ajuste a las columnas 0 a 5 y las filas necesarias
            anchor.Col1 = 0; // Columna de inicio
            anchor.Row1 = 0; // Fila de inicio
                             // anchor.Col2 = 1; // Columna de fin
                             // anchor.Row2 = 2; // Ajusta esto para que la imagen se ajuste mejor
            anchor.AnchorType = AnchorType.MoveDontResize;

            var picture = patriarch.CreatePicture(anchor, pictureIndex);
            picture.Resize();//RESIZE PICTURE
            // END CONFIG IMG LOGO

            // Crear estilo para fondo de todas las celdas
            var backgroundStyle = workBook.CreateCellStyle();
            backgroundStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
            backgroundStyle.FillPattern = FillPattern.SolidForeground;

            // Aplicar el estilo de fondo a todas las celdas
            for (int rowNum = 0; rowNum <= 10; rowNum++)
            {
                IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
                for (int colNum = 0; colNum < row.LastCellNum; colNum++)
                {
                    ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
                    cell.CellStyle = backgroundStyle;
                    sheet.AutoSizeColumn(colNum);
                }
            }

            //STYLE FOR BODY DATA
            var styleBody = workBook.CreateCellStyle();
            styleBody.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index;
            styleBody.FillPattern = FillPattern.SolidForeground;
            styleBody.Alignment = HorizontalAlignment.Center;
            //END STYLE FOR BODY DATA

            //HEADER PRINCIPAL STYLE
            var headerMainStyle = workBook.CreateCellStyle();
            headerMainStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
            headerMainStyle.FillPattern = FillPattern.SolidForeground;
            headerMainStyle.Alignment = HorizontalAlignment.Left;

            //HEADER STYLE
            var headerStyle = workBook.CreateCellStyle();
            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.Alignment = HorizontalAlignment.Center;
            //END HEADER STYLE

            //HEADER TITLES
            var rowHeader = sheet.CreateRow(6);
            rowHeader.CreateCell(0).SetCellValue("Number Document");
            rowHeader.CreateCell(1).SetCellValue("Payment Type");
            rowHeader.CreateCell(2).SetCellValue("Date Registry");
            rowHeader.CreateCell(3).SetCellValue("Total Sale");
            rowHeader.CreateCell(4).SetCellValue("Product");
            rowHeader.CreateCell(5).SetCellValue("Price Product");
            rowHeader.CreateCell(6).SetCellValue("Quantity");
            rowHeader.CreateCell(7).SetCellValue("Total");

            var colHeaderC0 = sheet.CreateRow(0);
            colHeaderC0.CreateCell(2).SetCellValue("GREATESHOP-REPORT");
            colHeaderC0.GetCell(2).CellStyle = headerStyle;
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 2, 7));



            var colHeaderC1 = sheet.CreateRow(1);
            colHeaderC1.CreateCell(2).SetCellValue("Date Range Found");
            colHeaderC1.GetCell(2).CellStyle = headerMainStyle;

            await Task.WhenAll(productAsync, pymentTypeMostUsed, range, totakSales);

            var product = productAsync.Result;
            colHeaderC1.CreateCell(3).SetCellValue(range.Result);
            colHeaderC1.GetCell(3).CellStyle = styleBody;

            colHeaderC1.CreateCell(4).SetCellValue("");
            colHeaderC1.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 7));

            var colHeaderC2 = sheet.CreateRow(2);
            colHeaderC2.CreateCell(2).SetCellValue("Total Revenue");
            colHeaderC2.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC2.CreateCell(3).SetCellValue("$" + totakSales.Result);
            colHeaderC2.GetCell(3).CellStyle = styleBody;

            colHeaderC2.CreateCell(4).SetCellValue("");
            colHeaderC2.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 7));

            var colHeaderC3 = sheet.CreateRow(3);
            colHeaderC3.CreateCell(2).SetCellValue("Most Used Payment Method");
            colHeaderC3.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC3.CreateCell(3).SetCellValue(pymentTypeMostUsed.Result);
            colHeaderC3.GetCell(3).CellStyle = styleBody;

            colHeaderC3.CreateCell(4).SetCellValue("");
            colHeaderC3.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 4, 7));



            var colHeaderC4 = sheet.CreateRow(4);
            colHeaderC4.CreateCell(2).SetCellValue("Best-Selling Product");
            colHeaderC4.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC4.CreateCell(3).SetCellValue(product[0]);
            colHeaderC4.GetCell(3).CellStyle = styleBody;

            colHeaderC4.CreateCell(4).SetCellValue("");
            colHeaderC4.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 7));

            var colHeaderC5 = sheet.CreateRow(5);
            colHeaderC5.CreateCell(2).SetCellValue("Product Quantity");
            colHeaderC5.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC5.CreateCell(3).SetCellValue(product[1]);
            colHeaderC5.GetCell(3).CellStyle = styleBody;

            colHeaderC5.CreateCell(4).SetCellValue("");
            colHeaderC5.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(5, 5, 4, 7));


            //CREATION HEADER STYLE IN ROWS          

            for (int i = 0; i < rowHeader.LastCellNum; i++)
            {
                rowHeader.GetCell(i).CellStyle = headerStyle;
                sheet.AutoSizeColumn(i);
            }
            //END HEADER TITLES 

            //BODY DATA



            var startRow = 7;
            foreach (var report in listReports)
            {
                var rowBody = sheet.CreateRow(startRow);
                ICell cell0 = rowBody.CreateCell(0);
                cell0.SetCellValue(report.numberDocument);
                cell0.SetCellType(CellType.String);

                ICell cell1 = rowBody.CreateCell(1);
                cell1.SetCellValue(report.PaymentType);
                cell1.SetCellType(CellType.String);

                ICell cell2 = rowBody.CreateCell(2);
                cell2.SetCellValue(report.DateRegistry);
                cell2.SetCellType(CellType.String);

                ICell cell3 = rowBody.CreateCell(3);
                cell3.SetCellValue(report.TotalSale);
                cell3.SetCellType(CellType.String);

                ICell cell4 = rowBody.CreateCell(4);
                cell4.SetCellValue(report.Product);
                cell4.SetCellType(CellType.String);

                ICell cell5 = rowBody.CreateCell(5);
                cell5.SetCellValue(report.Price);
                cell5.SetCellType(CellType.String);

                ICell cell6 = rowBody.CreateCell(6);
                cell6.SetCellValue(report.quantity);
                cell6.SetCellType(CellType.String);

                ICell cell7 = rowBody.CreateCell(7);
                cell7.SetCellValue(report.Total);
                cell7.SetCellType(CellType.String);

                for (int i = 0; i < rowBody.LastCellNum; i++)
                {
                    rowBody.GetCell(i).CellStyle = styleBody;
                    sheet.AutoSizeColumn(i);
                }
                startRow++;
            }
            //END BODY DATA

            var streamFile = new MemoryStream();
            workBook.Write(streamFile);

            fileExcel = streamFile.ToArray();
            if (files.ContainsKey(idUser))
            {
                files[idUser] = fileExcel;
            }
            else
            {
                files.Add(idUser, fileExcel);
            }

            isRunning[idUser] = false;
        }

        public async Task<string> GetDateRange(List<ReportDTO> listReports)
        {

            if (listReports == null || listReports.Count == 0)
            {
                return "No dates provided";
            }
            var listStrings = listReports.Select(x => x.DateRegistry).ToList();

            List<DateTime> dates = new List<DateTime>();

            foreach (var dateString in listStrings)
            {
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    dates.Add(date);
                }
                else
                {
                    return $"Invalid date format in: {dateString}";
                }
            }

            DateTime minDate = dates.Min();
            DateTime maxDate = dates.Max();

            return $"{minDate.ToString("dd/MM/yyyy")} - {maxDate.ToString("dd/MM/yyyy")}";
        }

        public async Task<string> getTotalSales(List<ReportDTO> listReports)
        {
            try
            {
                var uniqueReports = listReports
                                    .GroupBy(r => r.numberDocument)
                                    .Select(g => g.First())
                                    .ToList();

                var sumTotal = uniqueReports.Sum(x => Convert.ToDecimal(x.TotalSale));

                return sumTotal.ToString();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> getMostPymentTypeUsed(List<ReportDTO> listReports)
        {
            try
            {
                // Primero, agrupar por numberDocument para eliminar duplicados
                var uniqueDocuments = listReports
                    .GroupBy(report => report.numberDocument)
                    .Select(group => group.First())
                    .ToList();

                // Luego, agrupar por PaymentType y contar la frecuencia de cada tipo de pago
                var mostUsedPaymentType = uniqueDocuments
                    .GroupBy(report => report.PaymentType)
                    .Select(group => new
                    {
                        PaymentType = group.Key,
                        Count = group.Count()
                    })
                    .OrderByDescending(group => group.Count)
                    .FirstOrDefault();

                return mostUsedPaymentType?.PaymentType ?? "No payment type found";

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<string>> getMostSale(List<ReportDTO> listReports)
        {

            List<string> mostSale = new List<string>();
            try
            {
                var mostSoldProduct = listReports
               .GroupBy(report => report.Product) // Agrupar por Product
               .Select(group => new
               {
                   Product = group.Key,
                   TotalQuantity = group.Sum(report => int.Parse(report.quantity)) // Sumar la cantidad de cada grupo
               })
               .OrderByDescending(group => group.TotalQuantity) // Ordenar por TotalQuantity en orden descendente
               .FirstOrDefault(); // Tomar el primer grupo (el más grande)
                mostSale.Add(mostSoldProduct.Product);
                mostSale.Add(mostSoldProduct.TotalQuantity.ToString());


                return mostSale;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<byte[]> getFileExcel(int idUser)
        {
            if (isRunning.ContainsKey(idUser))
            {
                while (isRunning[idUser]) {
                    await Task.Delay(333);
                }
                return files[idUser];
            }
            return null;
            

        }

        public async Task runTaskFileExcel(int idUser)
        {
            DownloadExcel(idUser);

        }

        public bool DeleteFile(int idUser)
        {
            if (files.ContainsKey(idUser))
            {
                files.Remove(idUser);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<byte[]> GetExcelFile(List<ReportDTO> listReports)
        {
            //TASKS
            var productAsync = getMostSale(listReports);
            var pymentTypeMostUsed = getMostPymentTypeUsed(listReports);
            var range = GetDateRange(listReports);
            var totakSales = getTotalSales(listReports);

            var workBook = new XSSFWorkbook();

            var sheet = workBook.CreateSheet("Report");

            //CONFIG IMG LOGO
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var relativePath = "PicturesAndFiles" + Path.DirectorySeparatorChar + "logo.png";//GET IMAGE FROM PATH
            var filePath = Path.Combine(basePath, relativePath);
            pictureData = System.IO.File.ReadAllBytes(filePath);
            var pictureIndex = workBook.AddPicture(pictureData, NPOI.SS.UserModel.PictureType.PNG);//put the format
            var patriarch = sheet.CreateDrawingPatriarch();//ADD PICTURE TO WORKBOOK
            IClientAnchor anchor = workBook.GetCreationHelper().CreateClientAnchor();
            // Establecer el ancla de la imagen para que se ajuste a las columnas 0 a 5 y las filas necesarias
            anchor.Col1 = 0; // Columna de inicio
            anchor.Row1 = 0; // Fila de inicio
                             // anchor.Col2 = 1; // Columna de fin
                             // anchor.Row2 = 2; // Ajusta esto para que la imagen se ajuste mejor
            anchor.AnchorType = AnchorType.MoveDontResize;

            var picture = patriarch.CreatePicture(anchor, pictureIndex);
            picture.Resize();//RESIZE PICTURE
            // END CONFIG IMG LOGO

            // Crear estilo para fondo de todas las celdas
            var backgroundStyle = workBook.CreateCellStyle();
            backgroundStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
            backgroundStyle.FillPattern = FillPattern.SolidForeground;

            // Aplicar el estilo de fondo a todas las celdas
            for (int rowNum = 0; rowNum <= 10; rowNum++)
            {
                IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
                for (int colNum = 0; colNum < row.LastCellNum; colNum++)
                {
                    ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
                    cell.CellStyle = backgroundStyle;
                    sheet.AutoSizeColumn(colNum);
                }
            }

            //STYLE FOR BODY DATA
            var styleBody = workBook.CreateCellStyle();
            styleBody.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index;
            styleBody.FillPattern = FillPattern.SolidForeground;
            styleBody.Alignment = HorizontalAlignment.Center;
            //END STYLE FOR BODY DATA

            //HEADER PRINCIPAL STYLE
            var headerMainStyle = workBook.CreateCellStyle();
            headerMainStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
            headerMainStyle.FillPattern = FillPattern.SolidForeground;
            headerMainStyle.Alignment = HorizontalAlignment.Left;

            //HEADER STYLE
            var headerStyle = workBook.CreateCellStyle();
            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.Alignment = HorizontalAlignment.Center;
            //END HEADER STYLE

            //HEADER TITLES
            var rowHeader = sheet.CreateRow(6);
            rowHeader.CreateCell(0).SetCellValue("Number Document");
            rowHeader.CreateCell(1).SetCellValue("Payment Type");
            rowHeader.CreateCell(2).SetCellValue("Date Registry");
            rowHeader.CreateCell(3).SetCellValue("Total Sale");
            rowHeader.CreateCell(4).SetCellValue("Product");
            rowHeader.CreateCell(5).SetCellValue("Price Product");
            rowHeader.CreateCell(6).SetCellValue("Quantity");
            rowHeader.CreateCell(7).SetCellValue("Total");

            var colHeaderC0 = sheet.CreateRow(0);
            colHeaderC0.CreateCell(2).SetCellValue("GREATESHOP-REPORT");
            colHeaderC0.GetCell(2).CellStyle = headerStyle;
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 2, 7));



            var colHeaderC1 = sheet.CreateRow(1);
            colHeaderC1.CreateCell(2).SetCellValue("Date Range Found");
            colHeaderC1.GetCell(2).CellStyle = headerMainStyle;

            await Task.WhenAll(productAsync, pymentTypeMostUsed, range, totakSales);

            var product = productAsync.Result;
            colHeaderC1.CreateCell(3).SetCellValue(range.Result);
            colHeaderC1.GetCell(3).CellStyle = styleBody;

            colHeaderC1.CreateCell(4).SetCellValue("");
            colHeaderC1.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 7));

            var colHeaderC2 = sheet.CreateRow(2);
            colHeaderC2.CreateCell(2).SetCellValue("Total Revenue");
            colHeaderC2.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC2.CreateCell(3).SetCellValue("$" + totakSales.Result);
            colHeaderC2.GetCell(3).CellStyle = styleBody;

            colHeaderC2.CreateCell(4).SetCellValue("");
            colHeaderC2.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 7));

            var colHeaderC3 = sheet.CreateRow(3);
            colHeaderC3.CreateCell(2).SetCellValue("Most Used Payment Method");
            colHeaderC3.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC3.CreateCell(3).SetCellValue(pymentTypeMostUsed.Result);
            colHeaderC3.GetCell(3).CellStyle = styleBody;

            colHeaderC3.CreateCell(4).SetCellValue("");
            colHeaderC3.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 4, 7));



            var colHeaderC4 = sheet.CreateRow(4);
            colHeaderC4.CreateCell(2).SetCellValue("Best-Selling Product");
            colHeaderC4.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC4.CreateCell(3).SetCellValue(product[0]);
            colHeaderC4.GetCell(3).CellStyle = styleBody;

            colHeaderC4.CreateCell(4).SetCellValue("");
            colHeaderC4.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 7));

            var colHeaderC5 = sheet.CreateRow(5);
            colHeaderC5.CreateCell(2).SetCellValue("Product Quantity");
            colHeaderC5.GetCell(2).CellStyle = headerMainStyle;

            colHeaderC5.CreateCell(3).SetCellValue(product[1]);
            colHeaderC5.GetCell(3).CellStyle = styleBody;

            colHeaderC5.CreateCell(4).SetCellValue("");
            colHeaderC5.GetCell(4).CellStyle = styleBody;
            sheet.AddMergedRegion(new CellRangeAddress(5, 5, 4, 7));


            //CREATION HEADER STYLE IN ROWS          

            for (int i = 0; i < rowHeader.LastCellNum; i++)
            {
                rowHeader.GetCell(i).CellStyle = headerStyle;
                sheet.AutoSizeColumn(i);
            }
            //END HEADER TITLES 

            //BODY DATA



            var startRow = 7;
            foreach (var report in listReports)
            {
                var rowBody = sheet.CreateRow(startRow);
                ICell cell0 = rowBody.CreateCell(0);
                cell0.SetCellValue(report.numberDocument);
                cell0.SetCellType(CellType.String);

                ICell cell1 = rowBody.CreateCell(1);
                cell1.SetCellValue(report.PaymentType);
                cell1.SetCellType(CellType.String);

                ICell cell2 = rowBody.CreateCell(2);
                cell2.SetCellValue(report.DateRegistry);
                cell2.SetCellType(CellType.String);

                ICell cell3 = rowBody.CreateCell(3);
                cell3.SetCellValue(report.TotalSale);
                cell3.SetCellType(CellType.String);

                ICell cell4 = rowBody.CreateCell(4);
                cell4.SetCellValue(report.Product);
                cell4.SetCellType(CellType.String);

                ICell cell5 = rowBody.CreateCell(5);
                cell5.SetCellValue(report.Price);
                cell5.SetCellType(CellType.String);

                ICell cell6 = rowBody.CreateCell(6);
                cell6.SetCellValue(report.quantity);
                cell6.SetCellType(CellType.String);

                ICell cell7 = rowBody.CreateCell(7);
                cell7.SetCellValue(report.Total);
                cell7.SetCellType(CellType.String);

                for (int i = 0; i < rowBody.LastCellNum; i++)
                {
                    rowBody.GetCell(i).CellStyle = styleBody;
                    sheet.AutoSizeColumn(i);
                }
                startRow++;
            }
            //END BODY DATA

            var streamFile = new MemoryStream();
            workBook.Write(streamFile);

            fileExcel = streamFile.ToArray();

            return fileExcel;

        }
    }
}
