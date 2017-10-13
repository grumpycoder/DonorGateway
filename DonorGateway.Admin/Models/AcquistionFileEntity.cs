using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace DonorGateway.Admin.Models
{
    public class AcquistionFileEntity : TableEntity
    {
        public AcquistionFileEntity(Guid fileId)
        {
            PartitionKey = "acquisition";
            RowKey = fileId.ToString();
        }

        public AcquistionFileEntity() { }

        public string Filename { get; set; }
        public string Filesize { get; set; }
        public Int64 FileRecordCount { get; set; }
        public Int64 ProcessedRecordCount { get; set; }
        public double TotalProcessTime { get; set; }
        public DateTime? DateProcessed { get; set; }
        public int CampaignId { get; set; }
        public string Campaign { get; set; }
        public string Status { get; set; }
    }
}