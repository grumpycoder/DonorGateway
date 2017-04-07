using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CsvHelper;
using DonorGateway.Admin.Helpers;
using DonorGateway.Admin.ViewModels;
using DonorGateway.Data;
using DonorGateway.Domain;
using EntityFramework.Utilities;

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/event")]
    public class EventController : ApiController
    {
        private readonly DataContext context;

        public EventController()
        {
            context = DataContext.Create();
        }

        public IHttpActionResult Get()
        {
            var list = context.Events.Include(x => x.Template).ToList();

            return Ok(list);
        }

        [Route("{name}")]
        public IHttpActionResult Get(string name)
        {
            var vm = context.Events.AsQueryable().SingleOrDefault(x => x.Name == name);
            return Ok(vm);
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var @event = context.Events.Include(t => t.Template).SingleOrDefault(x => x.Id == id);
            if (@event == null) return NotFound();


            var attendanceCount = context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsWaiting == false).Sum(g => g.TicketCount);
            @event.GuestAttendanceCount = attendanceCount ?? 0;
            var waitingCount = context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsWaiting == true).Sum(g => g.TicketCount);
            @event.GuestWaitingCount = waitingCount ?? 0;
            @event.TicketMailedCount = context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsMailed).Sum(g => g.TicketCount) ?? 0;
            context.SaveChanges();

            var model = Mapper.Map<EventViewModel>(@event);
            return Ok(model);
        }

        [HttpPost, Route("{id:int}/guests")]
        public IHttpActionResult Guests(int id, GuestSearchViewModel vm)
        {
            var page = vm.Page.GetValueOrDefault(0);
            var pageSize = vm.PageSize.GetValueOrDefault(10);
            var skipRows = (page - 1) * pageSize;
            var ticketMailed = vm.IsMailed ?? false;
            var isWaiting = vm.IsWaiting ?? false;
            var isAttending = vm.IsAttending ?? false;

            var baseQuery = context.Guests.Where(e => e.EventId == id);

            var pred = PredicateBuilder.True<Guest>();
            if (!string.IsNullOrWhiteSpace(vm.Address)) pred = pred.And(p => p.Address.Contains(vm.Address));
            if (!string.IsNullOrWhiteSpace(vm.FinderNumber)) pred = pred.And(p => p.FinderNumber.StartsWith(vm.FinderNumber));
            if (!string.IsNullOrWhiteSpace(vm.Name)) pred = pred.And(p => p.Name.Contains(vm.Name));
            if (!string.IsNullOrWhiteSpace(vm.City)) pred = pred.And(p => p.City.StartsWith(vm.City));
            if (!string.IsNullOrWhiteSpace(vm.State)) pred = pred.And(p => p.State.Equals(vm.State));
            if (!string.IsNullOrWhiteSpace(vm.ZipCode)) pred = pred.And(p => p.Zipcode.StartsWith(vm.ZipCode));
            if (!string.IsNullOrWhiteSpace(vm.Phone)) pred = pred.And(p => p.Phone.Contains(vm.Phone));
            if (!string.IsNullOrWhiteSpace(vm.Email)) pred = pred.And(p => p.Email.StartsWith(vm.Email));
            if (!string.IsNullOrWhiteSpace(vm.LookupId)) pred = pred.And(p => p.LookupId.StartsWith(vm.LookupId));
            if (!string.IsNullOrWhiteSpace(vm.ConstituentType)) pred = pred.And(p => p.ConstituentType.StartsWith(vm.ConstituentType));
            if (vm.TicketCount != null) pred = pred.And(p => p.TicketCount == vm.TicketCount);
            if (vm.IsMailed != null) pred = pred.And(p => p.IsMailed == ticketMailed);
            if (vm.IsWaiting != null) pred = pred.And(p => p.IsWaiting == isWaiting);
            if (vm.IsAttending != null) pred = pred.And(p => p.IsAttending == isAttending);

            var filteredQuery = baseQuery.Where(pred);

            var list = filteredQuery
                .Order(vm.OrderBy, vm.OrderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending)
                .Where(pred)
                .Skip(skipRows)
                .Take(pageSize)
                .ProjectTo<GuestViewModel>();

            var totalCount = baseQuery.Count();
            var filterCount = filteredQuery.Count();
            var totalPages = (int)Math.Ceiling((decimal)filterCount / pageSize);

            vm.TotalCount = totalCount;
            vm.FilteredCount = filterCount;
            vm.TotalPages = totalPages;

            vm.Items = list.ToList();

            return Ok(vm);

        }

        [HttpPost, Route("{id:int}/guests/export")]
        public IHttpActionResult Export(int id, GuestSearchViewModel vm)
        {
            var ticketMailed = vm.IsMailed ?? false;
            var isWaiting = vm.IsWaiting ?? false;
            var isAttending = vm.IsAttending ?? false;

            var pred = PredicateBuilder.True<Guest>();
            pred = pred.And(p => p.EventId == id);
            if (!string.IsNullOrWhiteSpace(vm.Address)) pred = pred.And(p => p.Address.Contains(vm.Address));
            if (!string.IsNullOrWhiteSpace(vm.FinderNumber)) pred = pred.And(p => p.FinderNumber.StartsWith(vm.FinderNumber));
            if (!string.IsNullOrWhiteSpace(vm.Name)) pred = pred.And(p => p.Name.Contains(vm.Name));
            if (!string.IsNullOrWhiteSpace(vm.City)) pred = pred.And(p => p.City.StartsWith(vm.City));
            if (!string.IsNullOrWhiteSpace(vm.State)) pred = pred.And(p => p.State.Equals(vm.State));
            if (!string.IsNullOrWhiteSpace(vm.ZipCode)) pred = pred.And(p => p.Zipcode.StartsWith(vm.ZipCode));
            if (!string.IsNullOrWhiteSpace(vm.Phone)) pred = pred.And(p => p.Phone.Contains(vm.Phone));
            if (!string.IsNullOrWhiteSpace(vm.Email)) pred = pred.And(p => p.Email.StartsWith(vm.Email));
            if (!string.IsNullOrWhiteSpace(vm.LookupId)) pred = pred.And(p => p.LookupId.StartsWith(vm.LookupId));
            if (vm.IsMailed != null) pred = pred.And(p => p.IsMailed == ticketMailed);
            if (vm.IsWaiting != null) pred = pred.And(p => p.IsWaiting == isWaiting);
            if (vm.IsAttending != null) pred = pred.And(p => p.IsAttending == isAttending);

            var list = context.Guests.AsQueryable()
                    .Where(pred)
                    .ProjectTo<GuestExportViewModel>();

            var path = HttpContext.Current.Server.MapPath(@"~\app_data\guestlist.csv");

            using (var csv = new CsvWriter(new StreamWriter(File.Create(path))))
            {
                csv.Configuration.RegisterClassMap<GuestExportMap>();
                csv.WriteHeader<GuestExportViewModel>();
                csv.WriteRecords(list);
            }
            var filename = $"guest-list-{DateTime.Now.ToString("u")}.csv";

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename
            };
            response.Content.Headers.Add("x-filename", filename);

            return ResponseMessage(response);
        }

        [HttpDelete, Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var @event = context.Events.Find(id);
            if (@event == null) return NotFound();

            var template = context.Templates.Find(@event.TemplateId);

            if (template != null) context.Templates.Remove(template);
            context.SaveChanges();

            context.Events.Remove(@event);
            context.SaveChanges();

            return Ok("Deleted Event");
        }

        public IHttpActionResult Post(Event vm)
        {
            context.Templates.Add(vm.Template);
            context.SaveChanges();
            context.Events.Add(vm);
            context.SaveChanges();

            var @event = Mapper.Map<EventViewModel>(vm);

            return Ok(@event);
        }

        public IHttpActionResult Put(Event vm)
        {
            context.Events.AddOrUpdate(vm);
            context.SaveChanges();

            var model = Mapper.Map<EventViewModel>(vm);

            return Ok(model);
        }

        [HttpPost, Route("{id:int}/CancelRegistration")]
        public IHttpActionResult CancelRegistration(int id, Guest dto)
        {
            var @event = context.Events.Find(id);

            if (@event == null) return NotFound();

            @event.CancelRegistration(dto);

            context.Entry(@event.Template).State = EntityState.Unchanged;

            context.Events.AddOrUpdate(@event);

            context.Guests.AddOrUpdate(dto);

            dto.Event = @event;
            context.SaveChanges();

            return Ok(dto);
        }

        [HttpPost, Route("{id:int}/registerguest")]
        public IHttpActionResult RegisterGuest(int id, Guest dto)
        {
            var @event = context.Events.Find(id);
            var guest = context.Guests.Find(dto.Id);

            if (@event == null) return NotFound();

            @event.RegisterGuest(dto);
            @event.SendEmail(dto);

            if (guest != null)
            {
                if (!guest.Equals(dto))
                {
                    var demoChange = Mapper.Map<DemographicChange>(dto);
                    demoChange.Source = Source.RSVP;
                    context.DemographicChanges.Add(demoChange);
                }
            }

            context.Entry(@event.Template).State = EntityState.Unchanged;

            dto.Event = @event;

            context.Events.AddOrUpdate(@event);

            context.Guests.AddOrUpdate(dto);
            context.SaveChanges();

            return Ok(dto);
        }

        [HttpPost, Route("{id:int}/addticket")]
        public IHttpActionResult AddTicket(int id, GuestViewModel dto)
        {
            var @event = context.Events.Find(id);

            if (@event == null) return NotFound();
            if (dto == null) return NotFound();

            var guest = Mapper.Map<Guest>(dto);
            var current = context.Guests.Find(dto.Id);

            if (current != guest)
            {
                var demoChange = Mapper.Map<DemographicChange>(guest);
                demoChange.Source = Source.RSVP;
                context.DemographicChanges.Add(demoChange);
            }

            @event.AddTickets(guest, dto.AdditionalTickets);

            context.Events.AddOrUpdate(@event);

            context.Guests.AddOrUpdate(guest);
            context.SaveChanges();

            Mapper.Map(guest, dto);
            dto.Event = @event;
            return Ok(dto);
        }

        [HttpPut, Route("{id:int}/mailalltickets")]
        public IHttpActionResult MailAllTickets(int id)
        {
            //var @event = context.Events.Include(g => g.Guests).SingleOrDefault(e => e.Id == id);
            var @event = context.Events.SingleOrDefault(e => e.Id == id);

            if (@event == null) return NotFound();

            var list = context.Guests.Where(x => x.EventId == id && x.IsMailed == false && x.IsAttending == true && x.IsWaiting == false);

            //TODO: Event should do this
            foreach (var guest in list)
            {
                guest.IsMailed = true;
                guest.MailedDate = DateTime.Now;
                @event.TicketMailedCount++;
            }
            context.Events.AddOrUpdate(@event);
            context.SaveChanges();

            EFBatchOperation.For(context, context.Guests).UpdateAll(@event.Guests.ToList(), x => x.ColumnsToUpdate(c => c.IsMailed, c => c.MailedDate));
            context.SaveChanges();

            @event.TicketMailedCount = context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsWaiting == false).Sum(g => g.TicketCount) ?? 0;
            context.SaveChanges();

            return Ok(@event);
        }

        [HttpPut, Route("{id:int}/mailallwaiting")]
        public IHttpActionResult MailAllWaiting(int id)
        {
            var @event = context.Events.SingleOrDefault(e => e.Id == id);

            if (@event == null) return NotFound();

            var list = context.Guests.Where(x => x.EventId == id && x.IsMailed == false && x.IsAttending == true && x.IsWaiting == true);

            //TODO: Event should do this
            foreach (var guest in list)
            {
                guest.IsMailed = true;
                guest.MailedDate = DateTime.Now;
            }
            context.Events.AddOrUpdate(@event);
            context.SaveChanges();

            EFBatchOperation.For(context, context.Guests).UpdateAll(@event.Guests.ToList(), x => x.ColumnsToUpdate(c => c.IsMailed, c => c.MailedDate));
            context.SaveChanges();

            context.SaveChanges();

            return Ok(@event);
        }

        [HttpPost, Route("{id:int}/mailticket")]
        public IHttpActionResult MailTicket(int id, Guest dto)
        {
            var @event = context.Events.Find(id);

            if (@event == null) return NotFound();

            if (dto == null) return NotFound();

            @event.MailTicket(dto);

            dto.Event = @event;

            context.Events.AddOrUpdate(@event);
            context.SaveChanges();

            context.Guests.AddOrUpdate(dto);
            context.SaveChanges();

            return Ok(dto);
        }

        [HttpPost, Route("{id:int}/addtomail")]
        public IHttpActionResult AddToMail(int id, Guest dto)
        {
            var @event = context.Events.Find(id);

            if (@event == null) return NotFound();

            if (dto == null) return NotFound();

            @event.MoveToMailQueue(dto);
            dto.Event = @event;

            context.Events.AddOrUpdate(@event);
            context.SaveChanges();

            context.Guests.AddOrUpdate(dto);
            context.SaveChanges();

            return Ok(dto);
        }

    }
}