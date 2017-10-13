using CsvHelper.Configuration;
using DonorGateway.Domain;

namespace AcquisitionProcessor.Models
{
    internal class MailerMap : CsvClassMap<Mailer>
    {
        public MailerMap()
        {
            Map(m => m.FinderNumber).Name("Finder Number", "FinderNumber");
            Map(m => m.SourceCode).Name("Source Code", "SourceCode");
            Map(m => m.FirstName).Name("First Name", "FirstName");
            Map(m => m.MiddleName).Name("Middle Name", "MiddleName");
            Map(m => m.LastName).Name("Last Name", "LastName");
            Map(m => m.Address).Name("Address Line 1", "AddressLine1");
            Map(m => m.Address2).Name("Address Line 2", "AddressLine2");
            Map(m => m.Address3).Name("Address Line 3", "AddressLine3");
            Map(m => m.City).Name("City");
            Map(m => m.State).Name("State Abbreviation", "StateAbreviation");
            Map(m => m.ZipCode).Name("Zip Code", "ZipCode");
        }

    }
}