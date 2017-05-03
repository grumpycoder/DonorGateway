using System;
using AutoMapper;
using DonorGateway.Domain;
using Heroic.AutoMapper;

namespace DonorGateway.RSVP.ViewModels
{
    public class EventViewModel : IMapFrom<Event>, IHaveCustomMappings
    {
        public string PromoCode { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Speaker { get; set; }
        public string Venue { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public int Capacity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? VenueOpenDate { get; set; }
        public DateTime? RegistrationCloseDate { get; set; }
        public int? TicketAllowance { get; set; }
        public bool IsCancelled { get; set; }

        public int TicketMailedCount { get; set; }
        public int GuestWaitingCount { get; set; }
        public int GuestAttendanceCount { get; set; }

        public bool IsExpired => EndDate < DateTime.Now;
        public bool IsAtCapacity => TicketRemainingCount <= 0;
        public bool IsRegistrationClosed => RegistrationCloseDate <= DateTime.Now;

        public int TicketRemainingCount
        {
            get
            {
                var remaining = Capacity - GuestAttendanceCount;
                if (remaining < 0) remaining = 0;
                return remaining;
            }


        }

        public int TemplateId { get; set; }
        public Template Template { get; set; }


        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<Event, EventViewModel>()
                .ForMember(d => d.EventId, src => src.MapFrom(s => s.Id))
                ;
        }
    }

}

