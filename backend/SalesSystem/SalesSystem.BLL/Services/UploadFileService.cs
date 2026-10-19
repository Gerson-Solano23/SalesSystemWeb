using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.EntityFrameworkCore;

using NPOI.HPSF;
using NPOI.Util;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.Entity;
using SalesSystem.Utility;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class UploadFileService : IUploadS3File
    {
        private readonly static string AWSKEY = Environment.GetEnvironmentVariable("AWSKEY");
        private readonly static string AWSECRETKEY = Environment.GetEnvironmentVariable("AWSKEYSECRET");
        private readonly static string AWSBUCKETNAME = Environment.GetEnvironmentVariable("BUCKETNAME");
        // Optional S3-compatible endpoint for local development (e.g. LocalStack: http://localstack:4566).
        private readonly static string? AWS_S3_SERVICE_URL = Environment.GetEnvironmentVariable("AWS_S3_SERVICE_URL");
        private readonly static string AWS_REGION = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-2";

        /// <summary>
        /// Real AWS S3 by default; when AWS_S3_SERVICE_URL is set, an S3-compatible server such as
        /// LocalStack (path-style URLs, any credentials).
        /// </summary>
        private static AmazonS3Client CreateS3Client()
        {
            if (string.IsNullOrWhiteSpace(AWS_S3_SERVICE_URL))
            {
                return new AmazonS3Client(AWSKEY, AWSECRETKEY, RegionEndpoint.GetBySystemName(AWS_REGION));
            }

            var config = new AmazonS3Config
            {
                ServiceURL = AWS_S3_SERVICE_URL,
                ForcePathStyle = true,
                AuthenticationRegion = AWS_REGION
            };
            return new AmazonS3Client(AWSKEY, AWSECRETKEY, config);
        }

        private const int MaxFileSizeBytes = 10 * 1024 * 1024;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".txt", ".csv", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"
        };

        private readonly IGenericRepository<S3DetailsFile> _uploadFileService;

        /// <summary>
        /// Fails with a clear message when S3 is not configured (e.g. a local Docker setup),
        /// instead of an AWS SDK error.
        /// </summary>
        private static void EnsureStorageConfigured()
        {
            if (string.IsNullOrWhiteSpace(AWSKEY) || string.IsNullOrWhiteSpace(AWSECRETKEY) || string.IsNullOrWhiteSpace(AWSBUCKETNAME))
            {
                throw new BusinessException("File storage (S3) is not configured on the server: set AWSKEY, AWSKEYSECRET and BUCKETNAME");
            }
        }

        /// <summary>
        /// Rejects names with paths or unexpected extensions so clients can only address plain file keys.
        /// </summary>
        private static string ValidateFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName || fileName.Contains(".."))
            {
                throw new BusinessException("Invalid file name");
            }

            if (!AllowedExtensions.Contains(Path.GetExtension(fileName)))
            {
                throw new BusinessException("File type not allowed");
            }

            return fileName;
        }

        private async Task<S3DetailsFile> FindRegisteredFile(string fileName)
        {
            var query = await _uploadFileService.Consult(x => x.FileName == fileName);
            return await query.FirstOrDefaultAsync() ?? throw BusinessException.NotFound("File not found");
        }


        public UploadFileService(IGenericRepository<S3DetailsFile> uploadFileService)
        {
            _uploadFileService = uploadFileService;
        }

        public async Task<bool> UploadFile(byte[] fileData, string fileName, string fileContentType)
        {
            if (fileData == null || fileData.Length == 0 || fileName == null || fileContentType == null)
            {
                return false;
            }
            ValidateFileName(fileName);
            EnsureStorageConfigured();
            if (fileData.Length > MaxFileSizeBytes)
            {
                throw new BusinessException("File exceeds the 10 MB limit");
            }
            var existing = await _uploadFileService.Consult(x => x.FileName == fileName);
            if (await existing.AnyAsync())
            {
                throw new BusinessException("A file with the same name already exists");
            }
            try
            {
                using (var amazonS3client = CreateS3Client())
                {
                    /*
                     Para subir un archivo se requiere
                        #1 El previo cliente de amazon s3 en este caso con sus credenciales 
                        #2 El MemoryStream para pasarlo al InputStream
                        #3 El TransferUtilityUploadRequest para inicializarlo con el InputStream, Key, BucketName y ContentType
                        #4 El Nombre del Bucket y la data perteneciente al archivo a ingresar
                        #5 El TransferUtility el cual se necesita para realizar la transferencia, se le pasa el Cliente de amazon s3 
                        #6 Por ultimo al objeto inicializado de TransferUtility se utiliza la funcion / interfaz de UploadAsync pasandole el el objeto inicializado de 
                            TransferUtilityUploadRequest
                    */
                    using (var memoryStream = new MemoryStream(fileData))
                    {
                        var request = new TransferUtilityUploadRequest
                        {
                            InputStream = memoryStream,
                            Key = fileName,
                            BucketName = AWSBUCKETNAME,
                            ContentType = fileContentType
                        };

                        var transferUtility = new TransferUtility(amazonS3client);
                        await transferUtility.UploadAsync(request);
                    }

                    //using (MemoryStream stream = new MemoryStream(fileData))
                    //{


                    //    var request = new TransferUtilityUploadRequest
                    //    {
                    //        InputStream = stream,
                    //        Key = fileName,
                    //        BucketName = AWSBUCKETNAME,
                    //        ContentType = fileContentType,

                    //    };
                    //}
                    //var transferUtility = new TransferUtility(amazonS3client);
                    //await transferUtility.UploadAsync(request);

                }
                S3DetailsFile fileDetails = new S3DetailsFile();
                fileDetails.FileDate = DateTime.UtcNow;
                fileDetails.FileName = fileName;
                fileDetails.FileType = fileContentType;


                S3DetailsFile file = await _uploadFileService.CreateAsync(fileDetails);
                if (file != null)
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> DeleteFile(string fileName)
        {
            bool result = false;
            try
            {
                S3DetailsFile fileDetails = await FindRegisteredFile(fileName);
                EnsureStorageConfigured();
                var response = await _uploadFileService.DeleteAsync(fileDetails);
                //_dbContext.S3DetailsFile.Remove(fileDetails);
                if (response == true)
                {
                    using (var amazonS3client = CreateS3Client())
                    {
                        var transferUtility = new TransferUtility(amazonS3client);
                        await transferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                        {
                            BucketName = AWSBUCKETNAME,
                            Key = fileName

                        });
                        result = true;
                    }
                }

                return result;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> DownloadFile(string fileName)
        {
            // Only files registered through this system can be downloaded, not any key in the bucket.
            await FindRegisteredFile(fileName);
            EnsureStorageConfigured();

            //Autenticacion con AmazonS3Client 
            using (var amazonS3client = CreateS3Client())
            {
                //Inicializamos el objeto  TransferUtility y le añadimos el cliente de amazon s3 con sus credenciales
                var transferUtility = new TransferUtility(amazonS3client);
                //Dependiendo cada caso aqui se procede con el CRUD del archivo dentro del bucket 
                var response = await transferUtility.S3Client.GetObjectAsync(new GetObjectRequest()
                {
                    //Añadimos el nombre del Bucket al que queremos usar
                    BucketName = AWSBUCKETNAME,
                    //Luego le pasamos el key o nombre del archivo
                    Key = fileName,
                });

                if (response.ResponseStream == null)
                {
                    return new byte[0];
                }

                // Convert the Stream to byte[]
                using (var memoryStream = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    byte[] fileData = memoryStream.ToArray();


                    return fileData;
                }
            }
        }

        public async Task<bool> FileExists(string fileName)
        {
            var query = await _uploadFileService.Consult(x => x.FileName == fileName);
            return await query.AnyAsync();
        }

        public async Task<List<int>> getYearsWithFiles()
        {
            var query = await _uploadFileService.Consult(x => x.FileDate != null);
            var years = await query.Select(x => x.FileDate!.Value.Year).Distinct().ToListAsync();
            years.Add(DateTime.Now.Year);
            return years.Distinct().OrderByDescending(y => y).ToList();
        }

        public async Task<List<string>> getListFilesPerMonth(string month, string year)
        {
            if (!int.TryParse(month, out int monthNumber) || !int.TryParse(year, out int yearNumber)
                || monthNumber < 1 || monthNumber > 12 || yearNumber < 1 || yearNumber > 9999)
            {
                throw new BusinessException("Invalid month or year");
            }

            var start = new DateTime(yearNumber, monthNumber, 1);
            var endExclusive = start.AddMonths(1);
            var list = await _uploadFileService.Consult(x => x.FileDate >= start && x.FileDate < endExclusive);
            return await list.OrderBy(x => x.FileDate).Select(x => x.FileName).ToListAsync();
        }
    }
}
