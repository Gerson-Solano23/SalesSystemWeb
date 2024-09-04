using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NPOI.HPSF;
using NPOI.Util;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.Entity;
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

        private readonly IGenericRepository<S3DetailsFile> _uploadFileService;


        public UploadFileService(IGenericRepository<S3DetailsFile> uploadFileService)
        {
            _uploadFileService = uploadFileService;
        }

        public async Task<bool> UploadFile(byte[] fileData, string fileName, string fileContentType)
        {
            if (fileData == null || fileName == null || fileContentType == null)
            {
                return false;
            }
            try
            {
                using (var amazonS3client = new AmazonS3Client(AWSKEY, AWSECRETKEY, RegionEndpoint.USEast2))
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
                S3DetailsFile fileDetails = new S3DetailsFile();

                var query = await _uploadFileService.Consult();
                fileDetails = query.FirstOrDefaultAsync(x => x.FileName == fileName).Result;
                var response = await _uploadFileService.DeleteAsync(fileDetails);
                //_dbContext.S3DetailsFile.Remove(fileDetails);
                if (response == true)
                {
                    using (var amazonS3client = new AmazonS3Client(AWSKEY, AWSECRETKEY, RegionEndpoint.USEast2))
                    {
                        var transferUtility = new TransferUtility(amazonS3client);
                        await transferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                        {
                            BucketName = AWSBUCKETNAME,
                            Key = fileName

                        });
                    }
                }

                

                result = true;



                return result;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> DownloadFile(string fileName)
        {
            //Autenticacion con AmazonS3Client 
            using (var amazonS3client = new AmazonS3Client(AWSKEY, AWSECRETKEY, RegionEndpoint.USEast2))
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

        public async Task<List<string>> getListFilesPerMonth(string month, string year)
        {
            try
            {
                var list = await _uploadFileService.Consult();
                return list.Where(x => x.FileDate.Value.Month.ToString() == month && x.FileDate.Value.Year.ToString() == year).Select(x => x.FileName).ToList();
                //Where(x => x.FileDate.Value.Month.ToString() == month && x.FileDate.Value.Year.ToString() == year).Select(x => x.FileName).ToListAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
