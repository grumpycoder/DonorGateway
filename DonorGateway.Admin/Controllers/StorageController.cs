using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage.Queue;

namespace DonorGateway.Admin.Controllers
{
    public class StorageController : Controller
    {
        // GET: Storage
        public ActionResult Index()
        {
            var storageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("acquisition");
            table.CreateIfNotExists();

            var acquisition = new FileEntity(Guid.NewGuid())
            {
                Filename = "TestFile1.csv",
                Filesize = "19MB",
                RecordCount = 1000000,
                ProcessedRecordCount = 0,
                TotalProcessTime = 0
            };

            var insertOperation = TableOperation.Insert(acquisition);
            table.Execute(insertOperation);

            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("acquisitionqueue");

            queue.CreateIfNotExists();

            var message = new CloudQueueMessage(acquisition.RowKey);
            queue.AddMessage(message);

            return View(acquisition);
        }
    }

    public class FileEntity : TableEntity
    {
        public FileEntity(Guid fileId)
        {
            PartitionKey = "acquisition";
            RowKey = fileId.ToString();
        }

        public FileEntity() { }

        public string Filename { get; set; }
        public string Filesize { get; set; }
        public Int64 RecordCount { get; set; }
        public Int64 ProcessedRecordCount { get; set; }
        public double TotalProcessTime { get; set; }
        public DateTime? DateProcessed { get; set; }
    }
}