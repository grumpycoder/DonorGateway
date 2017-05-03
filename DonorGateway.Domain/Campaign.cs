using System;

namespace DonorGateway.Domain
{
    public class Campaign: BaseEntity
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}