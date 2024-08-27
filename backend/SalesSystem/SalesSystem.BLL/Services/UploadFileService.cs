using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NPOI.HPSF;
using NPOI.Util;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.Entity;
using System;
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

        private readonly DBSalesContext _dbContext;
        private readonly IGenericRepository<S3DetailsFile> _uploadFileService;
        private IFormFile file;


        public UploadFileService(DBSalesContext dbContext, IGenericRepository<S3DetailsFile> uploadFileService)
        {
            _dbContext = dbContext;
            _uploadFileService = uploadFileService;           
        }

        public async Task<bool> UploadFile(IFormFile fileData)
        {
            this.file = fileData;
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
                    using (var memorystream = new MemoryStream())
                    {
                        this.file.CopyTo(memorystream);
                        var request = new TransferUtilityUploadRequest
                        {
                            InputStream = memorystream,
                            Key = this.file.FileName,
                            BucketName = AWSBUCKETNAME,
                            ContentType = this.file.ContentType,

                        };

                        var transferUtility = new TransferUtility(amazonS3client);
                        await transferUtility.UploadAsync(request);
                    }
                }
                S3DetailsFile fileDetails = new S3DetailsFile();
                fileDetails.FileName = this.file.FileName;
                fileDetails.FileType = this.file.ContentType;


                var file = await _uploadFileService.CreateAsync(fileDetails);
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
            try
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

                using (var context = new DBSalesContext())
                {

                    S3DetailsFile fileDetails = new S3DetailsFile();
                    fileDetails = await context.S3DetailsFile.FirstOrDefaultAsync(x => x.FileName.ToLower() == fileName.ToLower());

                    context.S3DetailsFile.Remove(fileDetails);
                    context.SaveChanges();
                }

                return true;

            }
            catch (Exception)
            {
                return false;

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

       
    }
}
