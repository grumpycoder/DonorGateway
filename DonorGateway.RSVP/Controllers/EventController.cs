using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DonorGateway.Data;
using DonorGateway.Domain;
using DonorGateway.RSVP.Helpers;
using DonorGateway.RSVP.Interfaces;
using DonorGateway.RSVP.Services;
using DonorGateway.RSVP.ViewModels;
#pragma warning disable 618

namespace DonorGateway.RSVP.Controllers
{
    public class EventController : Controller
    {
        private IEventService _eventService;
        private readonly DataContext _context;

        public EventController()
        {
            _eventService = new EventService(new EventRepository(DataContext.Create()));
            _context = DataContext.Create();
        }


        [Route("{id}")]
        public ActionResult Index(string id)
        {
            var @event = _eventService.GetEvent(id);
            if (@event == null) return View("EventNotFound");

            var dto = Mapper.Map<EventViewModel>(@event);
            return View(dto);
        }

        [HttpPost]
        public ActionResult Register(EventViewModel model)
        {
            var guest = _eventService.FindGuestByFinder(model.PromoCode);
            if (guest == null) ModelState.AddModelError("PromoCode", "Invalid Reservation Code");

            var @event = _eventService.GetEvent(model.EventId);
            if (@event == null) ModelState.AddModelError("Event", "No event scheduled for this location");

            model.Template = @event.Template;

            var register = Mapper.Map<RegisterFormViewModel>(guest);

            if (register != null && register.IsRegistered) ModelState.AddModelError("Attendance", "Already registered for event");

            if (register != null) register.Template = @event.Template;

            return ModelState.IsValid ? View(register) : View("Index", model);
        }

        [HttpPost]
        public ActionResult Confirm(RegisterFormViewModel dto)
        {
            var @event = _eventService.GetEvent(dto.EventId);

            if (!ModelState.IsValid)
            {
                dto.Template = @event.Template;
                return View("Register", dto);
            }

            var guest = _eventService.GetGuest(dto.GuestId);

            var d = Mapper.Map<Guest>(dto);
            if (guest != d)
            {
                var demo = new DemographicChange()
                {
                    LookupId = dto.LookupId,
                    FinderNumber = dto.PromoCode,
                    Name = dto.Name,
                    Email = dto.Name,
                    Phone = dto.Name,
                    Street = dto.Address,
                    Street2 = dto.Address2,
                    City = dto.City,
                    State = dto.State,
                    Zipcode = dto.Zipcode,
                    Source = Source.RSVP
                };
                _context.DemographicChanges.AddOrUpdate(demo);
                _context.SaveChanges();
            }

            Mapper.Map(dto, guest);

            @event.RegisterGuest(guest);
            @event.SendEmail(guest);

            _context.Events.AddOrUpdate(@event);
            _context.Guests.AddOrUpdate(guest);
            _context.SaveChanges();

            var model = Mapper.Map<FinishFormViewModel>(guest);
            var template = @event.Template;
            model.Template = TemplateHelper.ParseGuestTemplate(guest, template);

            return View("Finish", model);
        }

        public ActionResult Finish(FinishFormViewModel model)
        {
            return View(model);
        }

        public ActionResult EventNotFound()
        {
            return View();
        }

    }
}