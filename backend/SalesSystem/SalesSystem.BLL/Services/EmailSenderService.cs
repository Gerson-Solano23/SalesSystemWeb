using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Document = QuestPDF.Fluent.Document;
namespace SalesSystem.BLL.Services
{
    public class EmailSenderService : ISendEmail
    {
        private static string emailAdress = "greateindustries@outlook.com";
        private static string password = "c.o.m.oHKT2310";

        public async Task<bool> SendEmail(EmailDTO email)
        {
            bool result = true;
            if (email.Attached == null || email.Attached.Length == 0)
            {
                email.Attached = await createPDFFile(email);
            }
            var message = new MailMessage();
            Attachment attachment = new Attachment(new MemoryStream(email.Attached), "Report.pdf", "application/pdf");
            try
            {
                email.sender_User = emailAdress;
                email.password_Sender = password;
                using (SmtpClient client = new SmtpClient())
                {
                    client.Host = "smtp-mail.outlook.com";
                    client.Port = 587; // The Port SSL/TLS
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(email.sender_User, email.password_Sender);
                    client.EnableSsl = true; // Enable SSL/TLS

                    //Make the message and componet about that 
                    using (message = new MailMessage())
                    {
                        message.From = new MailAddress(email.sender_User);
                        message.To.Add(email.recipient_User);
                        message.Subject = email.Subject;
                        message.Body = email.Body;
                        message.Attachments.Add(attachment);
                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                // Release resources
                message.Dispose();
                attachment.Dispose();
            }

            return result;

        }

        public async Task<byte[]> createPDFFile(EmailDTO email)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            //Document creaion 
            var doc = Document.Create(document =>
            {
                //Page Creation
                document.Page(page =>
                {
                    //Header creation
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.ConstantItem(100).Height(60).Placeholder();
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("GREATESHOP S.A").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Escazu Franquisia 01-San José").FontSize(9);
                            col.Item().AlignCenter().Text("22142051 / 83697830").FontSize(9);
                            col.Item().AlignCenter().Text("contacto@grateshop.com").FontSize(9);

                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Border(1).BorderColor("#257272").AlignCenter().Text("REPORT: 001145").Bold();
                            col.Item().Border(1).BorderColor("#257272").Background("#257272").AlignCenter().Text("Boleta de venta").Bold().FontColor("#fff");
                            col.Item().Border(1).BorderColor("#257272").AlignCenter().Text("B0001 - 253");


                        });
                    });
                    //Content creation
                    page.Content().Padding(11).Column(col1 =>
                    {
                        col1.Item().Column(col2 =>
                        {
                            col2.Item().Text("Datos del cliente").Underline().Bold();
                            col2.Item().Text(txt =>
                            {
                                txt.Span("Nombre: ").SemiBold().FontSize(10);
                                txt.Span("Hector García Goméz").FontSize(10);
                            });
                            col2.Item().Text(txt =>
                            {
                                txt.Span("DNI: ").SemiBold().FontSize(10);
                                txt.Span("9695784636").FontSize(10);
                            });
                            col2.Item().Text(txt =>
                            {
                                txt.Span("Dirección: ").SemiBold().FontSize(10);
                                txt.Span("Cartago, los jardines ").FontSize(10);
                            });
                        });

                        int totalFinal = 0;
                        col1.Item().PaddingVertical(10).LineHorizontal(0.5f);
                        //Table Creation
                        col1.Item().Table(table =>
                        {
                            //This part make the definition of colums
                            table.ColumnsDefinition(colums =>
                            {
                                colums.RelativeColumn(0.3f);
                                colums.RelativeColumn(2.5f);
                                colums.RelativeColumn();
                                colums.RelativeColumn();
                                colums.RelativeColumn();
                            });
                            //This part is created to define the table titles
                            table.Header(header =>
                            {
                                header.Cell().Background("#257272").Padding(2).AlignCenter().Text("#").FontColor("#fff");
                                header.Cell().Background("#257272").Padding(2).Text("Product").FontColor("#fff");
                                header.Cell().Background("#257272").Padding(2).Text("Unit Price").FontColor("#fff");
                                header.Cell().Background("#257272").Padding(2).Text("Quantity").FontColor("#fff");
                                header.Cell().Background("#257272").Padding(2).Text("Total").FontColor("#fff");
                            });
                            int index = 1;

                            foreach (var item in Enumerable.Range(1, 45))
                            {
                                var quantity = Placeholders.Random.Next(1, 10);
                                var price = Placeholders.Random.Next(5, 15);
                                var total = quantity * price;
                                totalFinal += total;
                                table.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).AlignCenter().Text(index.ToString()).FontSize(12);

                                table.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(Placeholders.Label()).FontSize(12);

                                table.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text($"${price}").FontSize(12);

                                table.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(quantity.ToString()).FontSize(12);

                                table.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text($"${total}").FontSize(12);
                                index++;
                            }
                        });

                        col1.Item().AlignRight().PaddingRight(35).PaddingTop(10).Text($"Total: ${totalFinal}").FontSize(12);
                        col1.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                        {

                            col.Item().Text("Comentarios").FontSize(14);
                            if (col.Item() == null) return;
                            col.Item().Text(Placeholders.LoremIpsum()).FontSize(14);
                            col.Spacing(10);
                        });
                    });

                    //Footer creation
                    page.Footer().AlignRight().Padding(20).Text(txt =>
                    {
                        txt.Span("Page ").FontSize(10);
                        txt.CurrentPageNumber().FontSize(10);
                        txt.Span(" / ").FontSize(10);
                        txt.TotalPages().FontSize(10);
                    });
                });
            }).GeneratePdf();
            Stream pdfStream = new MemoryStream(doc);

            // Convertir el PDF en bytes
            byte[] bytesFile = ((MemoryStream)pdfStream).ToArray();
            // Cerrar el flujo de memoria
            pdfStream.Close();

            return bytesFile;
        }
    }
}
