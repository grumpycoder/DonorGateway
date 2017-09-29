using AutoMapper;
using DonorGateway.Domain;
using Heroic.AutoMapper;
using System;

namespace DonorGateway.Admin.ViewModels
{
    public class GuestViewModel : IMapFrom<Guest>, IHaveCustomMappings
    {
        public int Id { get; set; }
        public string LookupId { get; set; }
        public string FinderNumber { get; set; }
        public string ConstituentType { get; set; }
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
        public string Comment { get; set; }
        public string SPLCComment { get; set; }

        public int? TicketCount { get; set; }
        public bool IsMailed { get; set; } = false;

        public bool? IsAttending { get; set; } = false;
        public bool IsWaiting { get; set; } = false;

        public DateTime? ResponseDate { get; set; }
        public DateTime? MailedDate { get; set; }
        public DateTime? WaitingDate { get; set; }

        public int? EventId { get; set; }

        public int AdditionalTickets { get; set; }
        public int? TicketAllowance { get; set; }

        public bool CanRegister
        {
            get
            {
                if (IsAttending == null) return true;
                return (bool)!IsAttending && !IsWaiting;
            }
        }

        public bool CanMail
        {
            get
            {
                if (IsAttending == null) return false;

                return !IsMailed && (bool)IsAttending && !IsWaiting;
            }
        }

        public bool CanCancel
        {
            get
            {
                if (IsAttending == null) return false;
                return (bool)IsAttending;
            }
        }
        public bool CanAddTickets
        {
            get
            {
                if (IsAttending == null) return false;
                return (bool)IsAttending;
            }
        }

        public bool CanAddToAttending
        {
            get
            {
                //if (IsAttending == null) return false;
                return IsWaiting;
            }
        }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<Guest, GuestViewModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.TicketAllowance, opt => opt.MapFrom(s => s.Event.TicketAllowance))
                .ForMember(d => d.TicketCount, opt => opt.NullSubstitute(0))
                .ForMember(d => d.IsAttending, opt => opt.NullSubstitute(false))
                .ReverseMap();
        }
    }
}