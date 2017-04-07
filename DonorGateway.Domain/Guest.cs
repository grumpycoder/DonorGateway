using DonorGateway.Domain.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DonorGateway.Domain
{
    public class Guest : BaseEntity
    {
        public string LookupId { get; set; }
        public string FinderNumber { get; set; }
        public string ConstituentType { get; set; }
        public string DonorType { get; set; }
        public string SourceCode { get; set; }
        public string InteractionId { get; set; }

        public string MembershipYear { get; set; }
        public bool? LeadershipCouncil { get; set; }
        public string InsideSalutation { get; set; }
        public string OutsideSalutation { get; set; }
        public string HouseholdSalutation1 { get; set; }
        public string HouseholdSalutation2 { get; set; }
        public string HouseholdSalutation3 { get; set; }
        public string EmailSalutation { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }

        public int? TicketCount { get; set; }
        public bool IsMailed { get; set; }

        public bool? IsAttending { get; set; }
        public bool? IsWaiting { get; set; }

        public DateTime? ResponseDate { get; set; }
        public DateTime? MailedDate { get; set; }
        public DateTime? WaitingDate { get; set; }

        public string MailedBy { get; set; }

        public int? EventId { get; set; }

        public virtual Event Event { get; set; }

        //ADDED FOR IMPORT/EXPORT FIELDS
        public string ActualDate { get; set; }
        public string ExpectedDate { get; set; }
        public string Comment { get; set; }
        public string ResponseType { get; set; }
        public string SPLCComment { get; set; }
        public string Status { get; set; }
        public string ContactMethod { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Owner { get; set; }

        public string Parse(string message)
        {
            var properties = typeof(Guest).GetProperties().Where(p => p.PropertyType == typeof(DateTime?) || p.PropertyType == typeof(string));
            foreach (var prop in properties)
            {
                if (prop.GetValue(this, null) == null) continue;

                var propValue = prop.GetValue(this, null).ToString();
                if (prop.PropertyType == typeof(DateTime?))
                {
                    propValue = Convert.ToDateTime(prop.GetValue(this, null)).ToString("dddd, MMMM d, yyyy @ h:mm tt");
                }

                if (string.IsNullOrWhiteSpace(propValue)) continue;
                message = ReplaceText(message, prop.Name, propValue);
            }
            return message;
        }

        //TODO: Should be extension
        private static string ReplaceText(string stringToReplace, string fieldName, string fieldValue)
        {
            var pattern = "@{" + fieldName + "}";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(stringToReplace);

            return matches.Replace(stringToReplace, fieldValue);

        }

        protected bool Equals(Guest other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(LookupId, other.LookupId, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(FinderNumber, other.FinderNumber, StringComparison.OrdinalIgnoreCase) &&
                                                   ((Address ?? string.Empty) == (other.Address ?? string.Empty)) &&
                                                   ((Address2 ?? string.Empty) == (other.Address2 ?? string.Empty)) &&
                                                   ((Address3 ?? string.Empty) == (other.Address3 ?? string.Empty)) &&
                                                   string.Equals(City, other.City, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(State, other.State, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(Zipcode, other.Zipcode, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(Phone, other.Phone, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Guest)) return false;
            return Equals((Guest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Name) : 0);
                hashCode = (hashCode * 397) ^ (LookupId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(LookupId) : 0);
                hashCode = (hashCode * 397) ^ (FinderNumber != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(FinderNumber) : 0);
                hashCode = (hashCode * 397) ^ (Address != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Address) : 0);
                hashCode = (hashCode * 397) ^ (Address2 != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Address2) : 0);
                hashCode = (hashCode * 397) ^ (Address3 != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Address3) : 0);
                hashCode = (hashCode * 397) ^ (City != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(City) : 0);
                hashCode = (hashCode * 397) ^ (State != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(State) : 0);
                hashCode = (hashCode * 397) ^ (Zipcode != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Zipcode) : 0);
                hashCode = (hashCode * 397) ^ (Email != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Email) : 0);
                hashCode = (hashCode * 397) ^ (Phone != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Phone) : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Guest left, Guest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Guest left, Guest right)
        {
            return !Equals(left, right);
        }

        public virtual Guest Copy()
        {
            return MemberwiseClone() as Guest;
        }
    }
}