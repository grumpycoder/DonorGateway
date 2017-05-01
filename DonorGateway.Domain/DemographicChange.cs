namespace DonorGateway.Domain
{
    public class DemographicChange //: ConstituentDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LookupId { get; set; }
        public string FinderNumber { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Source Source { get; set; }
    }
}