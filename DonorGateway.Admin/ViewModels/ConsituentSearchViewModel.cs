namespace DonorGateway.Admin.ViewModels
{
    public class ConsituentSearchViewModel : Pager<ConstituentViewModel>
    {
        public string Name { get; set; }
        public string FinderNumber { get; set; }
        public string LookupId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}