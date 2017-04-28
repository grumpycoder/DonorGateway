using DonorGateway.Domain;
using System;
using System.Collections.Generic;
using Heroic.AutoMapper;

namespace DonorGateway.Admin.ViewModels
{
    public class EventViewModel : IMapFrom<Event>, IMapTo<Event>
    {
        public int Id { get; set; }
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
        public bool? IsCancelled { get; set; }

        public int TicketMailedCount { get; set; }
        public int GuestWaitingCount { get; set; }
        public int GuestAttendanceCount { get; set; }

        public bool IsExpired => EndDate < DateTime.Now;
        public bool IsAtCapacity => TicketRemainingCount <= 0;
        public int? TemplateId { get; set; }

        public int TicketRemainingCount
        {
            get
            {
                var remaining = Capacity - GuestAttendanceCount;
                if (remaining < 0) remaining = 0;
                return remaining;
            }


        }

        public Template Template { get; set; }

        public ICollection<Guest> Guests { get; set; }
    }
}