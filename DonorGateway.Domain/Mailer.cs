namespace DonorGateway.Domain
{
    public class Mailer: BaseEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string SourceCode { get; set; }
        public string FinderNumber { get; set; }
        public bool? Suppress { get; set; }
        public int? CampaignId { get; set; }
        public int? ReasonId { get; set; }

        public virtual Campaign Campaign { get; set; }
        public virtual SuppressReason Reason { get; set; }

    }
}