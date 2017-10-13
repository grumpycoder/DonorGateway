using DonorGateway.Admin.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace DonorGateway.Admin.Controllers
{
    [Route("api/acquisitions")]
    public class AcquistionsController : ApiController
    {
        private readonly CloudStorageAccount storageAccount;

        public AcquistionsController()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
        }

        public IHttpActionResult Get()
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("acquisitions");

            TableQuery<AcquistionFileEntity> query = new TableQuery<AcquistionFileEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "acquisition"));

            var list = table.ExecuteQuery(query).ToList();

            return Ok(list);
        }
    }
}
