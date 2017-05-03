using System.Collections.Generic;

namespace DonorGateway.Domain
{
    public class Constituent : ConstituentDetail
    {
        public Constituent()
        {
            TaxItems = new List<TaxItem>();
        }
        public List<TaxItem> TaxItems { get; set; }
    }
}