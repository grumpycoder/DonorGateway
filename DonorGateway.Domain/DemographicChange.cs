namespace DonorGateway.Domain
{
    public class DemographicChange : ConstituentDetail
    {
        public Source Source { get; set; }
    }

    public enum Source
    {
        Tax,
        RSVP
    }
}