using CsvHelper.Configuration;
using DonorGateway.Domain;
using System;
using DonorGateway.Admin.Helpers;

namespace DonorGateway.Admin.ViewModels
{
    public class TaxImportMap : CsvClassMap<CsvTaxRecord>
    {
        public TaxImportMap()
        {
            Map(m => m.LookupId).Name("LookupId", "LookupID", "Lookup Id", "Lookup ID", "lookupid", "lookup id", "lookup Id", "lookup ID");
            Map(m => m.FinderNumber)
                .Name("FinderNumber", "Finder Number", "finder number", "findernumber", "FinderNumber", "Findernumber");
            Map(m => m.Name).Name("Name", "name");
            Map(m => m.EmailAddress).Name("EmailAddress", "Email Address", "emailaddress", "email address", "Email", "email");
            Map(m => m.Addressline1).Name("Address line1", "Address line 1", "Addressline1", "address line1", "address line 1", "addressline1", "address line1", "Address Line 1", "AddressLine1", "Address Line1");
            Map(m => m.Addressline2).Name("Address line2", "Address line 2", "Addressline2", "address line2", "address line 2", "addressline2", "address line2", "Address Line 2", "AddressLine2", "Address Line2");
            Map(m => m.Addressline3).Name("Address line3", "Address line 3", "Addressline3", "address line3", "address line 3", "addressline3", "address line3", "Address Line 3", "AddressLine3", "Address Line3");
            Map(m => m.City).Name("City", "city");
            Map(m => m.State).Name("State", "state");
            Map(m => m.Zipcode).Name("ZIP", "Zip", "zip", "ZipCode", "Zipcode", "zipcode");
            Map(m => m.TaxYear).ConvertUsing(row => row.GetField("Date").ToDateTime().Year);
            Map(m => m.DonationDate).Name("Date", "DonationDate", "Donation Date", "Donation date", "donation Date", "donationdate", "Donationdate", "donationDate");
            Map(m => m.Amount)
                .Name("Amount", "DonationAmount", "Donation Amount", "Donation amount", "donation Amount", "donationamount", "Donationamount", "donationAmount")
                .ConvertUsing(row => Convert.ToDecimal(row.GetField("Amount").Replace("$", "")));
        }
    }
}
