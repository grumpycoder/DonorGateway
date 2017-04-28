using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
using EntityFramework.Extensions;

#pragma warning disable 618

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/event")]
    public class EventController : ApiController
    {
        private readonly DataContext _context;
        private const int PAGE_SIZE = 20;

        public EventController()
        {
            _context = DataContext.Create();
        }

        public async Task<object> Get()
        {
            var list = await _context.Events.ProjectTo<EventSummaryViewModel>().ToListAsync();
            return Ok(list);
        }

        [Route("{name}")]
        public async Task<object> Get(string name)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Name == name);
            if (model == null) return BadRequest("Event not found");

            var @event = Mapper.Map<EventSummaryViewModel>(model);
            return Ok(@event);
        }

        [HttpGet, Route("{id:int}")]
        public async Task<object> Get(int id)
        {
            var @event = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);
            if (@event == null) return NotFound();

            var attendanceCount = _context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsWaiting == false).Sum(g => g.TicketCount);
            @event.GuestAttendanceCount = attendanceCount ?? 0;
            var waitingCount = _context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsWaiting).Sum(g => g.TicketCount);
            @event.GuestWaitingCount = waitingCount ?? 0;
            @event.TicketMailedCount = _context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsMailed && !g.IsWaiting).Sum(g => g.TicketCount) ?? 0;
            _context.SaveChanges();

            var model = Mapper.Map<EventViewModel>(@event);
            return Ok(model);
        }

        [HttpGet, Route("{id:int}/guests")]
        public async Task<object> Guests(int id, [FromUri]GuestSearchModel pager)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            if (pager == null) pager = new GuestSearchModel();

            var ticketMailed = pager.IsMailed ?? false;
            var isWaiting = pager.IsWaiting ?? false;
            var isAttending = pager.IsAttending ?? false;
            var query = _context.Guests.Where(e => e.EventId == id);
            var totalCount = await query.CountAsync();

            var pred = PredicateBuilder.True<Guest>();
            if (!string.IsNullOrWhiteSpace(pager.Address)) pred = pred.And(p => p.Address.Contains(pager.Address));
            if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.StartsWith(pager.FinderNumber));
            if (!string.IsNullOrWhiteSpace(pager.Name)) pred = pred.And(p => p.Name.Contains(pager.Name));
            if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.City.StartsWith(pager.City));
            if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.Equals(pager.State));
            if (!string.IsNullOrWhiteSpace(pager.ZipCode)) pred = pred.And(p => p.Zipcode.StartsWith(pager.ZipCode));
            if (!string.IsNullOrWhiteSpace(pager.Phone)) pred = pred.And(p => p.Phone.Contains(pager.Phone));
            if (!string.IsNullOrWhiteSpace(pager.Email)) pred = pred.And(p => p.Email.StartsWith(pager.Email));
            if (!string.IsNullOrWhiteSpace(pager.LookupId)) pred = pred.And(p => p.LookupId.StartsWith(pager.LookupId));
            if (!string.IsNullOrWhiteSpace(pager.ConstituentType)) pred = pred.And(p => p.ConstituentType.StartsWith(pager.ConstituentType));
            if (pager.IsMailed != null) pred = pred.And(p => p.IsMailed == ticketMailed);
            if (pager.IsWaiting != null) pred = pred.And(p => p.IsWaiting == isWaiting);
            if (pager.IsAttending != null) pred = pred.And(p => p.IsAttending == isAttending);

            var filteredQuery = query.Where(pred);
            var pagerCount = await filteredQuery.CountAsync();
            var totalPages = Math.Ceiling((double)pagerCount / pager.PageSize ?? PAGE_SIZE);

            var results = await query.Where(pred)
                .Order(pager.OrderBy, pager.OrderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending)
                .Skip(pager.PageSize * (pager.Page - 1) ?? 0)
                .Take(pager.PageSize ?? PAGE_SIZE)
                .ProjectTo<GuestViewModel>().ToListAsync();

            pager.TotalCount = totalCount;
            pager.FilteredCount = pagerCount;
            pager.TotalPages = totalPages;
            pager.Results = results;
            stopwatch.Stop();
            pager.ElapsedTime = stopwatch.Elapsed;
            return Ok(pager);
        }

        //[HttpDelete, Route("{id}")]
        //public IHttpActionResult Delete(int id)
        //{
        //    var @event = _context.Events.Find(id);
        //    if (@event == null) return NotFound();

        //    var template = _context.Templates.Find(@event.TemplateId);

        //    if (template != null) _context.Templates.Remove(template);
        //    _context.SaveChanges();

        //    _context.Events.Remove(@event);
        //    _context.SaveChanges();

        //    return Ok("Deleted Event");
        //}

        //public IHttpActionResult Post(Event vm)
        //{
        //_context.Templates.Add(vm.Template);
        //_context.SaveChanges();
        //_context.Events.Add(vm);
        //_context.SaveChanges();

        //var @event = Mapper.Map<EventViewModel>(vm);

        //return Ok(@event);
        //}

        //public IHttpActionResult Put(Event vm)
        //{
        //    _context.Events.AddOrUpdate(vm);
        //    _context.SaveChanges();

        //    var model = Mapper.Map<EventViewModel>(vm);

        //    return Ok(model);
        //}

        [HttpPost, Route("{id:int}/register")]
        public async Task<object> RegisterGuest(int id, [FromBody]GuestViewModel model)
        {
            var @event = _context.Events.Find(id);
            if (@event == null) return BadRequest("Event not found");
            var guest = await _context.Guests.FirstOrDefaultAsync(e => e.Id == model.Id);
            if (guest == null) guest = Mapper.Map<Guest>(model);

            Mapper.Map(model, guest);

            @event.RegisterGuest(guest);
            @event.SendEmail(guest);

            //TODO: Create Demographic Change entry

            _context.Entry(@event.Template).State = EntityState.Unchanged;

            _context.Events.AddOrUpdate(@event);

            _context.Guests.AddOrUpdate(guest);
            _context.SaveChanges();

            var dto = Mapper.Map<GuestViewModel>(guest);
            return Ok(dto);
        }

        [HttpPost, Route("{id:int}/CancelRegister/{guestId:int}")]
        public async Task<object> CancelRegistration(int id, int guestId)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (@event == null) return BadRequest("Event not found");

            var guest = await _context.Guests.FirstOrDefaultAsync(e => e.Id == guestId);
            if (guest == null) return BadRequest("No guest found");

            @event.CancelRegistration(guest);

            //_context.Entry(@event.Template).State = EntityState.Unchanged;

            _context.Events.AddOrUpdate(@event);

            _context.Guests.AddOrUpdate(guest);

            _context.SaveChanges();
            var dto = Mapper.Map<GuestViewModel>(guest);
            return Ok(dto);
        }

        [HttpPost, Route("{id:int}/mailticket/{guestId:int}")]
        public async Task<object> MailTicket(int id, int guestId)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (@event == null) return BadRequest("Event not found");

            var guest = await _context.Guests.FirstOrDefaultAsync(e => e.Id == guestId);

            if (guest == null) return BadRequest("Guest not found");

            @event.MailTicket(guest);

            _context.Events.AddOrUpdate(@event);
            _context.Guests.AddOrUpdate(guest);
            _context.SaveChanges();

            var dto = Mapper.Map<GuestViewModel>(guest);
            return Ok(dto);
        }

        [HttpPost, Route("{id:int}/addticket")]
        public async Task<object> AddTicket(int id, GuestViewModel model)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (@event == null) return BadRequest("Event not found");

            var guest = await _context.Guests.FirstOrDefaultAsync(e => e.Id == model.Id);
            if (guest == null) return BadRequest("Guest not found");

            //TODO: Record demographic change update

            //var guest = Mapper.Map<Guest>(dto);
            //var current = _context.Guests.Find(dto.Id);

            //if (current != guest)
            //{
            //    var demoChange = Mapper.Map<DemographicChange>(guest);
            //    demoChange.Source = Source.RSVP;
            //    _context.DemographicChanges.Add(demoChange);
            //}

            @event.AddTickets(guest, model.AdditionalTickets);

            _context.Events.AddOrUpdate(@event);
            _context.Guests.AddOrUpdate(guest);
            _context.SaveChanges();

            Mapper.Map(guest, model);
            //dto.Event = @event;
            return Ok(model);
        }

        [HttpGet, Route("guest/{id:int}")]
        public async Task<object> GetGuest(int id)
        {
            var guest = await _context.Guests.Include("Event").ProjectTo<GuestViewModel>().FirstOrDefaultAsync(e => e.Id == id);
            if (guest == null) return BadRequest("Guest not found");
            return Ok(guest);
        }

        [HttpPost, Route("{id:int}/sendalltickets")]
        public async Task<object> SendAllTickets(int id)
        {
            try
            {
                var @event = await _context.Events.SingleOrDefaultAsync(e => e.Id == id);
                if (@event == null) return BadRequest("Event not found");

                _context.Guests
                    .Where(x => x.EventId == id && x.IsMailed == false && x.IsAttending == true && x.IsWaiting == false)
                    .Update(t => new Guest() { IsMailed = true, MailedDate = DateTime.Now });
                _context.SaveChanges();

                @event.TicketMailedCount = _context.Guests.Where(g => g.EventId == id && g.IsAttending == true && g.IsWaiting == false).Sum(g => g.TicketCount) ?? 0;
                _context.SaveChanges();

                return Ok(@event);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost, Route("{id:int}/sendallwaiting")]
        public async Task<object> SendAllWaiting(int id)
        {
            try
            {
                var @event = await _context.Events.SingleOrDefaultAsync(e => e.Id == id);
                if (@event == null) return NotFound();

                _context.Guests
                    .Where(x => x.EventId == id && x.IsMailed == false && x.IsAttending == true && x.IsWaiting == true)
                    .Update(t => new Guest() { IsMailed = true, MailedDate = DateTime.Now });
                _context.SaveChanges();

                _context.Events.AddOrUpdate(@event);
                _context.SaveChanges();

                return Ok(@event);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //[HttpPost, Route("{id:int}/guests/export")]
        //public async Task<object> Export(int id, [FromUri]GuestSearchModel pager)
        [HttpGet, Route("{id:int}/guests/export")]
        public async Task<object> Export(int id, [FromUri]GuestSearchModel pager)
        {
            try
            {
                if (pager == null) pager = new GuestSearchModel();

                var ticketMailed = pager.IsMailed ?? false;
                var isWaiting = pager.IsWaiting ?? false;
                var isAttending = pager.IsAttending ?? false;
                var query = _context.Guests.Where(e => e.EventId == id);

                var pred = PredicateBuilder.True<Guest>();
                if (!string.IsNullOrWhiteSpace(pager.Address)) pred = pred.And(p => p.Address.Contains(pager.Address));
                if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.StartsWith(pager.FinderNumber));
                if (!string.IsNullOrWhiteSpace(pager.Name)) pred = pred.And(p => p.Name.Contains(pager.Name));
                if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.City.StartsWith(pager.City));
                if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.Equals(pager.State));
                if (!string.IsNullOrWhiteSpace(pager.ZipCode)) pred = pred.And(p => p.Zipcode.StartsWith(pager.ZipCode));
                if (!string.IsNullOrWhiteSpace(pager.Phone)) pred = pred.And(p => p.Phone.Contains(pager.Phone));
                if (!string.IsNullOrWhiteSpace(pager.Email)) pred = pred.And(p => p.Email.StartsWith(pager.Email));
                if (!string.IsNullOrWhiteSpace(pager.LookupId)) pred = pred.And(p => p.LookupId.StartsWith(pager.LookupId));
                if (!string.IsNullOrWhiteSpace(pager.ConstituentType)) pred = pred.And(p => p.ConstituentType.StartsWith(pager.ConstituentType));
                if (pager.IsMailed != null) pred = pred.And(p => p.IsMailed == ticketMailed);
                if (pager.IsWaiting != null) pred = pred.And(p => p.IsWaiting == isWaiting);
                if (pager.IsAttending != null) pred = pred.And(p => p.IsAttending == isAttending);

                var filteredQuery = query.Where(pred);

                var results = await filteredQuery
                    .ProjectTo<GuestExportViewModel>().ToListAsync();

                var path = HttpContext.Current.Server.MapPath(@"~\app_data\guestlist.csv");

                using (var csv = new CsvWriter(new StreamWriter(File.Create(path))))
                {
                    csv.Configuration.RegisterClassMap<GuestExportMap>();
                    csv.WriteHeader<GuestExportViewModel>();
                    csv.WriteRecords(results);
                }
                var filename = $"guest-list-{DateTime.Now:u}.csv";

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
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            //var ticketMailed = vm.IsMailed ?? false;
            //var isWaiting = vm.IsWaiting ?? false;
            //var isAttending = vm.IsAttending ?? false;

            //var pred = PredicateBuilder.True<Guest>();
            //pred = pred.And(p => p.EventId == id);
            //if (!string.IsNullOrWhiteSpace(vm.Address)) pred = pred.And(p => p.Address.Contains(vm.Address));
            //if (!string.IsNullOrWhiteSpace(vm.FinderNumber)) pred = pred.And(p => p.FinderNumber.StartsWith(vm.FinderNumber));
            //if (!string.IsNullOrWhiteSpace(vm.Name)) pred = pred.And(p => p.Name.Contains(vm.Name));
            //if (!string.IsNullOrWhiteSpace(vm.City)) pred = pred.And(p => p.City.StartsWith(vm.City));
            //if (!string.IsNullOrWhiteSpace(vm.State)) pred = pred.And(p => p.State.Equals(vm.State));
            //if (!string.IsNullOrWhiteSpace(vm.ZipCode)) pred = pred.And(p => p.Zipcode.StartsWith(vm.ZipCode));
            //if (!string.IsNullOrWhiteSpace(vm.Phone)) pred = pred.And(p => p.Phone.Contains(vm.Phone));
            //if (!string.IsNullOrWhiteSpace(vm.Email)) pred = pred.And(p => p.Email.StartsWith(vm.Email));
            //if (!string.IsNullOrWhiteSpace(vm.LookupId)) pred = pred.And(p => p.LookupId.StartsWith(vm.LookupId));
            //if (vm.IsMailed != null) pred = pred.And(p => p.IsMailed == ticketMailed);
            //if (vm.IsWaiting != null) pred = pred.And(p => p.IsWaiting == isWaiting);
            //if (vm.IsAttending != null) pred = pred.And(p => p.IsAttending == isAttending);

            //var list = _context.Guests.AsQueryable()
            //    .Where(pred)
            //    .ProjectTo<GuestExportViewModel>();

            //var path = HttpContext.Current.Server.MapPath(@"~\app_data\guestlist.csv");

            //using (var csv = new CsvWriter(new StreamWriter(File.Create(path))))
            //{
            //    csv.Configuration.RegisterClassMap<GuestExportMap>();
            //    csv.WriteHeader<GuestExportViewModel>();
            //    csv.WriteRecords(list);
            //}
            //var filename = $"guest-list-{DateTime.Now.ToString("u")}.csv";

            //var response = new HttpResponseMessage(HttpStatusCode.OK);
            //var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            //response.Content = new StreamContent(stream);
            //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = filename
            //};
            //response.Content.Headers.Add("x-filename", filename);

            //return ResponseMessage(response);
        }


    }
}