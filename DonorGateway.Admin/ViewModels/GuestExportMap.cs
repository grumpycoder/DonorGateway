using CsvHelper.Configuration;

namespace DonorGateway.Admin.ViewModels
{
    public sealed class GuestExportMap : CsvClassMap<GuestExportViewModel>
    {
        public GuestExportMap()
        {
            Map(m => m.FinderNumber).Name("FinderNumber");
            Map(m => m.LookupId).Name("LookupId");
            Map(m => m.ConstituentType).Name("ConstituentType");
            Map(m => m.DonorType).Name("DonorType");
            Map(m => m.SourceCode).Name("SourceCode");
            Map(m => m.InteractionId).Name("InteractionId");
            Map(m => m.MembershipYear).Name("MembershipYear");
            Map(m => m.LeadershipCouncil).Name("LeadershipCouncil");

            Map(m => m.InsideSalutation).Name("InsideSalutation");
            Map(m => m.OutsideSalutation).Name("OutsideSalutation");
            Map(m => m.HouseholdSalutation1).Name("HouseholdSalutation1");
            Map(m => m.HouseholdSalutation2).Name("HouseholdSalutation2");
            Map(m => m.HouseholdSalutation3).Name("HouseholdSalutation3");
            Map(m => m.EmailSalutation).Name("EmailSalutation");

            Map(m => m.Name).Name("ConstituentName", "ConstituentName");
            Map(m => m.Email).Name("Email");
            Map(m => m.Phone).Name("Phone");

            Map(m => m.Address).Name("Address1", "Address1");
            Map(m => m.Address2).Name("Address2");
            Map(m => m.Address3).Name("Address3");
            Map(m => m.City).Name("City");
            Map(m => m.State).Name("State");
            Map(m => m.StateName).Name("StateName");
            Map(m => m.Zipcode).Name("Zipcode");
            Map(m => m.Country).Name("Country");

            Map(m => m.TicketCount).Name("TicketCount").Default(0);
            Map(m => m.IsMailed).Name("TicketMailed").Default(false);
            Map(m => m.Attending).Name("Attending");

            Map(m => m.ActualDate).Name("ActualDate");
            Map(m => m.ExpectedDate).Name("ExpectedDate");
            Map(m => m.MailedDate).Name("MailedDate");
            Map(m => m.WaitingDate).Name("WaitingDate");
            Map(m => m.Comment).Name("DonorComment");
            Map(m => m.Response).Name("Response");
            Map(m => m.ResponseType).Name("ResponseType");
            Map(m => m.SPLCComment).Name("SPLCComment");
            Map(m => m.Status).Name("Status");
            Map(m => m.ContactMethod).Name("ContactMethod");
            Map(m => m.Summary).Name("Summary");
            Map(m => m.Category).Name("Category");
            Map(m => m.SubCategory).Name("SubCategory");
            Map(m => m.Owner).Name("Owner");

            Map(m => m.EventName).Name("EventName");
            Map(m => m.EventCode).Name("EventCode");



        }
    }
}