using CsvHelper;
using CsvHelper.Configuration;
using DonorGateway.Admin.Models;
using DonorGateway.Admin.ViewModels;
using DonorGateway.Data;
using DonorGateway.Domain;
using EntityFramework.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/file")]
    public class FileController : ApiController
    {
        private readonly DataContext _context;
        private static CloudStorageAccount _cloudStorageAccount;
        private static string _storageConnectionString;

        public FileController()
        {
            _context = DataContext.Create();
            _storageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            _cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
        }

        [HttpPost, Route("mailer/{id:int}")]
        public object Mailer(int id)
        {
            try
            {

                var context = DataContext.Create();
                var campaign = context.Campaigns.Find(id);

                //Load file into blob storage 
                // Create the blob client.
                CloudBlobClient blobClient = _cloudStorageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference("acquisitions");
                container.CreateIfNotExists();

                var postedFile = HttpContext.Current.Request.Files[0];
                // Fix for IE file path issue.
                var filename = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                var filePath = HttpContext.Current.Server.MapPath(@"~\app_data\" + filename);
                postedFile.SaveAs(filePath);

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
                using (var fileStream = File.OpenRead(filePath))
                {
                    blockBlob.UploadFromStream(fileStream);
                }


                //Load record into table storage 
                var tableClient = _cloudStorageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference("acquisitions");
                table.CreateIfNotExists();

                var acquisition = new AcquistionFileEntity(Guid.NewGuid())
                {
                    Filename = filename,
                    CampaignId = id,
                    Campaign = campaign?.Name ?? "Unknown"
                };

                var insertOperation = TableOperation.Insert(acquisition);
                table.Execute(insertOperation);

                //Add to queue for processing
                var queueClient = _cloudStorageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference("acquisitions");

                queue.CreateIfNotExists();

                var queueMessage = new CloudQueueMessage(acquisition.RowKey);

                queue.AddMessage(queueMessage);

                //Delete server upload file
                File.Delete(filePath);

                return Ok($"Queued file for processing for {campaign?.Name}");

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        private static void CreateFileBlob(string filePath)
        {
            // Create the blob client.
            var blobClient = _cloudStorageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            var containerName = ConfigurationManager.AppSettings["Container"];
            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            //// Fix for IE file path issue.
            var filename = filePath.Substring(filePath.LastIndexOf("\\", StringComparison.Ordinal) + 1);

            var blockBlob = container.GetBlockBlobReference(filename);
            using (var fileStream = File.OpenRead(filePath))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }

        private static void CreateTableRecord(AcquistionFileEntity acquisition)
        {
            var tableClient = _cloudStorageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("acquistions");

            table.CreateIfNotExists();

            var insertOperation = TableOperation.Insert(acquisition);

            table.Execute(insertOperation);
        }


        [HttpPost, Route("guest/{id:int}")]
        public object Guest(int id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Restart();

            try
            {
                var postedFile = HttpContext.Current.Request.Files[0];
                // Fix for IE file path issue.
                var filename = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                var filePath = HttpContext.Current.Server.MapPath(@"~\app_data\" + filename);
                postedFile.SaveAs(filePath);

                var configuration = new CsvConfiguration()
                {
                    IsHeaderCaseSensitive = false,
                    WillThrowOnMissingField = false,
                    IgnoreReadingExceptions = true,
                    ThrowOnBadData = false,
                    SkipEmptyRecords = true
                };
                var csv = new CsvReader(new StreamReader(filePath, Encoding.Default, true), configuration);

                csv.Configuration.RegisterClassMap<GuestImportMap>();
                var list = csv.GetRecords<Guest>().ToList();
                foreach (var guest in list) guest.EventId = id;

                using (_context)
                {
                    EFBatchOperation.For(_context, _context.Guests).InsertAll(list);
                }

                var message = $"Processed {list.Count} records";
                stopwatch.Stop();
                var result = new OperationResult(true, message, stopwatch.Elapsed);

                csv.Dispose();
                File.Delete(filePath);
                return Ok(result);

            }
            catch (Exception ex)
            {
                var message = $"Error occurred processing records. {ex.Message}";
                return BadRequest(message);
            }

        }



        //[HttpPost, Route("tax")]
        //public IHttpActionResult Tax()
        //{
        //    var ImportStoredProcName = "ProcessTaxStaging";
        //    var httpRequest = HttpContext.Current.Request;
        //    var startTime = DateTime.Now;
        //    try
        //    {
        //        var postedFile = httpRequest.Files[0];
        //        // Fix for IE file path issue.
        //        var filename = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
        //        var filePath = HttpContext.Current.Server.MapPath(@"~\app_data\" + filename);
        //        postedFile.SaveAs(filePath);

        //        var configuration = new CsvConfiguration()
        //        {
        //            IsHeaderCaseSensitive = false,
        //            WillThrowOnMissingField = false,
        //            IgnoreReadingExceptions = true,
        //            ThrowOnBadData = false,
        //            SkipEmptyRecords = true,
        //            TrimHeaders = true
        //        };
        //        var csv = new CsvReader(new StreamReader(filePath, Encoding.Default, true), configuration);

        //        csv.Configuration.RegisterClassMap<TaxImportMap>();
        //        var list = csv.GetRecords<CsvTaxRecord>().ToList();
        //        using (_context)
        //        {
        //            EFBatchOperation.For(_context, _context.CsvTaxRecord).InsertAll(list);
        //        }

        //        var message = $"Processed {list.Count} records<br />";
        //        //var result = new OperationResult(true, message, DateTime.Now.Subtract(startTime));

        //        csv.Dispose();
        //        using (var db = new DataContext())
        //        {
        //            var usernameParameter = new SqlParameter("@Username", User.Identity.Name);
        //            var result = db.Database.SqlQuery<TaxImportProcessResult>($"{ImportStoredProcName} @username", usernameParameter).ToList();
        //            var r = result.FirstOrDefault();
        //            message +=
        //                $"Added {r.ConstituentInsertCount} Constituents | Updated {r.ConstituentUpdateCount} Constituents | Added {r.TaxInsertCount} New Tax Records";
        //        }

        //        File.Delete(filePath);
        //        return Ok(message);

        //    }
        //    catch (Exception ex)
        //    {
        //        var message = $"Error occurred processing records. {ex.Message}";
        //        return BadRequest(message);
        //    }

        //}


    }


}