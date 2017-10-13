using AcquisitionProcessor.Models;
using CsvHelper;
using CsvHelper.Configuration;
using DonorGateway.Data;
using DonorGateway.Domain;
using EntityFramework.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AcquisitionProcessor
{
    public class Functions
    {

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called acquisitions.
        public void ProcessAcquisitionMessage([QueueTrigger("acquisitions")] string id, TextWriter logger)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Restart();

            logger.WriteLine($"Starting queue process.");
            logger.Flush();

            var context = DataContext.Create();
            context.Configuration.AutoDetectChangesEnabled = false;

            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("acquisitions");

            var retrieveOperation = TableOperation.Retrieve<AcquistionFileEntity>("acquisition", id);
            var fileEntity = (AcquistionFileEntity)table.Execute(retrieveOperation).Result;

            try
            {
                if (fileEntity == null)
                {
                    logger.WriteLine($"File not found for {id}");
                    logger.Flush();
                    throw new Exception($"File not found for {id}");
                }
                fileEntity.Status = "processing";

                var ms = new MemoryStream();
                // Create the blob client.
                var blobClient = storageAccount.CreateCloudBlobClient();
                // Retrieve reference to a previously created container.
                var container = blobClient.GetContainerReference("acquisitions");
                // Retrieve reference to a blob named "myblob.txt"
                var blob = container.GetBlockBlobReference(fileEntity.Filename);

                logger.WriteLine($"Downloading {fileEntity.Filename} file from storage");
                logger.Flush();

                blob.DownloadToStream(ms);

                var configuration = new CsvConfiguration()
                {
                    IsHeaderCaseSensitive = false,
                    WillThrowOnMissingField = false,
                    IgnoreReadingExceptions = true,
                    ThrowOnBadData = false,
                    SkipEmptyRecords = true,
                    TrimHeaders = true
                };

                ms.Position = 0;
                var csv = new CsvReader(new StreamReader(ms, Encoding.Default, true), configuration);

                csv.Configuration.RegisterClassMap<MailerMap>();
                var list = csv.GetRecords<Mailer>().ToList();
                foreach (var mailer in list)
                {
                    mailer.CampaignId = fileEntity.CampaignId;
                    mailer.Suppress = false;
                }

                logger.WriteLine($"Importing {list.Count} records");
                logger.Flush();

                EFBatchOperation.For(context, context.Mailers).InsertAll(list, batchSize: 1000);

                logger.WriteLine($"Import complete");
                logger.Flush();

                var acquistionCount = context.Mailers.Count(m => m.CampaignId == fileEntity.CampaignId);

                csv.Dispose();
                ms.Dispose();

                stopWatch.Stop();

                fileEntity.FileRecordCount = list.Count;
                fileEntity.ProcessedRecordCount = acquistionCount;
                fileEntity.Status = "success";
                fileEntity.TotalProcessTime = stopWatch.Elapsed.Minutes;

                var updateOperation = TableOperation.Replace(fileEntity);
                // Execute the operation.
                table.Execute(updateOperation);
                logger.WriteLine($"Finished in {stopWatch.Elapsed.Minutes} minutes");
                logger.Flush();
            }
            catch (Exception e)
            {
                if (fileEntity != null)
                {
                    fileEntity.Status = "failed";
                    var updateOperation = TableOperation.Replace(fileEntity);
                    // Execute the operation.
                    table.Execute(updateOperation);
                }

                logger.WriteLine($"Failed {e.Message}");
                logger.Flush();
                throw;
            }

        }


    }
}
