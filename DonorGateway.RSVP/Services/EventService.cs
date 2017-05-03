using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonorGateway.Data;
using DonorGateway.Domain;
using DonorGateway.RSVP.Interfaces;

namespace DonorGateway.RSVP.Services
{
    public class EventService : IEventService
    {
        private EventRepository eventRepository;

        public EventService(EventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }
        public Event GetEvent(string name)
        {
            return eventRepository.GetEventByName(name);
        }

        public Event GetEvent(int eventId)
        {
            return eventRepository.GetEventById(eventId);
        }

        public Guest FindGuestByFinder(string finderNumber)
        {
            return eventRepository.GetGuestByFinder(finderNumber);
        }

        public Guest GetGuest(int guestId)
        {
            return eventRepository.GetGuestById(guestId);
        }
    }

    public class EventRepository
    {
        private readonly DataContext _context;

        public EventRepository(DataContext context)
        {
            this._context = context;
        }

        public Event GetEventByName(string name)
        {
            return _context.Events.FirstOrDefault(e => e.Name == name);
        }

        public Event GetEventById(int eventId)
        {
            return _context.Events.Include("Template").FirstOrDefault(e => e.Id == eventId);
        }

        public Guest GetGuestByFinder(string finderNumber)
        {
            return _context.Guests.SingleOrDefault(e => e.FinderNumber == finderNumber);
        }

        public Guest GetGuestById(int guestId)
        {
            return _context.Guests.Find(guestId);
        }
    }
}
