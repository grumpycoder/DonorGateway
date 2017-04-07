using System;

namespace DonorGateway.Domain
{
    public class CsvTaxRecord
    {
        public int Id { get; set; }
        public string LookupId { get; set; }
        public string FinderNumber { get; set; }
        public int ConstituentId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Addressline1 { get; set; }
        public string Addressline2 { get; set; }
        public string Addressline3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string DonationDate { get; set; }
        public decimal Amount { get; set; }
        public int TaxYear { get; set; }

        public string UpdatedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
