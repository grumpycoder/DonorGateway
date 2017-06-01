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

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/mailer")]
    public class MailerController : ApiController
    {

        private readonly DataContext _context;
        private const int PAGE_SIZE = 20;

        public MailerController()
        {
            _context = DataContext.Create();
        }

        public async Task<object> Get([FromUri] MailerSearchModel pager = null)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Restart();
                if (pager == null) pager = new MailerSearchModel();

                var query = _context.Mailers;
                var totalCount = await query.CountAsync();

                var pred = PredicateBuilder.True<Mailer>();
                if (!string.IsNullOrWhiteSpace(pager.FirstName)) pred = pred.And(p => p.FirstName.StartsWith(pager.FirstName));
                if (!string.IsNullOrWhiteSpace(pager.LastName)) pred = pred.And(p => p.LastName.StartsWith(pager.LastName));
                if (!string.IsNullOrWhiteSpace(pager.Address)) pred = pred.And(p => p.Address.Contains(pager.Address));
                if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.City.Contains(pager.City));
                if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.Equals(pager.State));
                if (!string.IsNullOrWhiteSpace(pager.ZipCode)) pred = pred.And(p => p.ZipCode.Contains(pager.ZipCode));
                if (!string.IsNullOrWhiteSpace(pager.SourceCode)) pred = pred.And(p => p.SourceCode.Equals(pager.SourceCode));
                if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.Equals(pager.FinderNumber));
                if (pager.CampaignId != null) pred = pred.And(p => p.CampaignId == pager.CampaignId);
                if (pager.ReasonId != null) pred = pred.And(p => p.ReasonId == pager.ReasonId);
                pred = pred.And(p => p.Suppress == pager.Suppress);

                var filteredQuery = query.Where(pred);
                var pagerCount = filteredQuery.Count();
                var totalPages = Math.Ceiling((double)pagerCount / pager.PageSize ?? PAGE_SIZE);

                var results = await filteredQuery.Where(pred)
                                           .Order(pager.OrderBy, pager.OrderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending)
                                           .Order("Id", SortDirection.Descending)
                                           .Skip(pager.PageSize * (pager.Page - 1) ?? 0)
                                           .Take(pager.PageSize ?? PAGE_SIZE)
                                           .ProjectTo<MailerViewModel>().ToListAsync();

                pager.TotalCount = totalCount;
                pager.FilteredCount = pagerCount;
                pager.TotalPages = totalPages;
                pager.Results = results;
                stopwatch.Stop();
                pager.ElapsedTime = stopwatch.Elapsed;
                return Ok(pager);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException.Message);
            }
        }

        [HttpGet, Route("campaigns")]
        public IHttpActionResult Campiagns()
        {
            var campaigns = _context.Campaigns.OrderByDescending(e => e.Id).ToList();
            return Ok(campaigns);
        }

        [HttpGet, Route("reasons")]
        public IHttpActionResult Reasons()
        {
            var reasons = _context.SuppressReasons.ToList();
            return Ok(reasons);
        }

        [HttpPost, Route("export")]
        public async Task<IHttpActionResult> Export(MailerSearchModel pager)
        {
            try
            {
                var pred = PredicateBuilder.True<Mailer>();
                if (!string.IsNullOrWhiteSpace(pager.FirstName)) pred = pred.And(p => p.FirstName.StartsWith(pager.FirstName));
                if (!string.IsNullOrWhiteSpace(pager.LastName)) pred = pred.And(p => p.LastName.StartsWith(pager.LastName));
                if (!string.IsNullOrWhiteSpace(pager.Address)) pred = pred.And(p => p.Address.Contains(pager.Address));
                if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.City.Contains(pager.City));
                if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.Contains(pager.State));
                if (!string.IsNullOrWhiteSpace(pager.ZipCode)) pred = pred.And(p => p.ZipCode.Contains(pager.ZipCode));
                if (!string.IsNullOrWhiteSpace(pager.SourceCode)) pred = pred.And(p => p.SourceCode.Contains(pager.SourceCode));
                if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.Contains(pager.FinderNumber));
                if (pager.CampaignId != null) pred = pred.And(p => p.CampaignId == pager.CampaignId);
                if (pager.ReasonId != null) pred = pred.And(p => p.ReasonId == pager.ReasonId);
                pred = pred.And(p => p.Suppress == pager.Suppress);

                var list = await _context.Mailers.AsQueryable()
                            .Where(pred)
                            .ProjectTo<MailerViewModel>().ToListAsync();

                var path = HttpContext.Current.Server.MapPath(@"~\app_data\mailerlist.csv");

                using (var csv = new CsvWriter(new StreamWriter(File.Create(path))))
                {
                    csv.Configuration.RegisterClassMap<MailerMap>();
                    csv.WriteHeader<MailerViewModel>();
                    csv.WriteRecords(list);
                }
                var filename = $"mailer-list-{DateTime.Now.ToString("u")}.csv";

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
                return BadRequest(e.InnerException.Message);
            }
        }

        [HttpPost, Route("CreateCampaign")]
        public IHttpActionResult CreateCampaign(Campaign campaign)
        {

            _context.Campaigns.Add(campaign);
            _context.SaveChanges();

            return Ok(campaign);
        }

        [HttpPut]
        public IHttpActionResult Put(Mailer mailer)
        {
            var m = _context.Mailers.Find(mailer.Id);
            if (m == null) return NotFound();

            _context.Mailers.AddOrUpdate(mailer);
            _context.SaveChanges();

            m = _context.Mailers.FirstOrDefault(e => e.Id == mailer.Id);
            var vm = Mapper.Map<MailerViewModel>(m);
            return Ok(vm);
        }

    }
}
