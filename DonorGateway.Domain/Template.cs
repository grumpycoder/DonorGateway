namespace DonorGateway.Domain
{
    public class Template : BaseEntity
    {
        public string Name { get; set; }
        public string HeaderText { get; set; }
        public string BodyText { get; set; }
        public string FooterText { get; set; }
        public string FAQText { get; set; }
        public string YesResponseText { get; set; }
        public string NoResponseText { get; set; }
        public string WaitingResponseText { get; set; }
        public string CancelledEventText { get; set; }
        public string ExpiredEventText { get; set; }
        public string Image { get; set; }
        public string MimeType { get; set; }

        public virtual Template Copy()
        {
            return MemberwiseClone() as Template;
        }

        public Template ParseTemplate(Guest guest)
        {
            return new Template();
        }
    }
}