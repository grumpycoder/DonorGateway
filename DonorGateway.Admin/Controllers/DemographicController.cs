using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using AutoMapper.QueryableExtensions;
using CsvHelper;
using DonorGateway.Admin.Helpers;
using DonorGateway.Admin.ViewModels;
using DonorGateway.Data;
using DonorGateway.Domain;
using EntityFramework.Utilities;

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/demographic")]
    public class DemographicController : ApiController
    {
        private readonly DataContext _context;
        private const int PAGE_SIZE = 20;

        public DemographicController()
        {
            _context = DataContext.Create();
        }

        public async Task<object> Get([FromUri]DemographicSearchViewModel pager)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Restart();
                if (pager == null) pager = new DemographicSearchViewModel();

                var query = _context.DemographicChanges;
                var totalCount = await query.CountAsync();

                var pred = PredicateBuilder.True<DemographicChange>();
                if (!string.IsNullOrWhiteSpace(pager.Name)) pred = pred.And(p => p.Name.Contains(pager.Name));
                if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.Contains(pager.FinderNumber));
                if (!string.IsNullOrWhiteSpace(pager.LookupId)) pred = pred.And(p => p.LookupId.Contains(pager.LookupId));
                if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.State.StartsWith(pager.City));
                if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.StartsWith(pager.State));
                if (!string.IsNullOrWhiteSpace(pager.Zipcode)) pred = pred.And(p => p.Zipcode.StartsWith(pager.Zipcode));
                if (!string.IsNullOrWhiteSpace(pager.Email)) pred = pred.And(p => p.Email.Contains(pager.Email));
                if (!string.IsNullOrWhiteSpace(pager.Phone)) pred = pred.And(p => p.Phone.Contains(pager.Phone));
                if (pager.Source != null) pred = pred.And(p => (int)p.Source == pager.Source);

                var filteredQuery = query.Where(pred);
                var pagerCount = filteredQuery.Count();
                var totalPages = Math.Ceiling((double)pagerCount / pager.PageSize ?? PAGE_SIZE);

                var results = await filteredQuery.Where(pred)
                    .Order(pager.OrderBy, pager.OrderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending)
                    .Skip(pager.PageSize * (pager.Page - 1) ?? 0)
                    .Take(pager.PageSize ?? PAGE_SIZE)
                    .ProjectTo<DemographicViewModel>().ToListAsync();

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
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete, Route("{id:int}")]
        public async Task<object> Delete(int id)
        {
            try
            {
                var demographic = await _context.DemographicChanges.FindAsync(id);
                if (demographic == null) return BadRequest("Demographic not found");

                _context.DemographicChanges.Remove(demographic);
                await _context.SaveChangesAsync();
                return Ok("Delete demographic");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public object Delete()
        {
            var count = EFBatchOperation.For(_context, _context.DemographicChanges).Where(e => true).Delete();
            return Ok($"Deleted {count}");
        }

        [HttpGet, Route("export")]
        public async Task<object> Export([FromUri]DemographicSearchViewModel pager)
        {
            try
            {
                if (pager == null) pager = new DemographicSearchViewModel();

                var query = _context.DemographicChanges;

                var pred = PredicateBuilder.True<DemographicChange>();
                if (!string.IsNullOrWhiteSpace(pager.Name)) pred = pred.And(p => p.Name.Contains(pager.Name));
                if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.Contains(pager.FinderNumber));
                if (!string.IsNullOrWhiteSpace(pager.LookupId)) pred = pred.And(p => p.LookupId.Contains(pager.LookupId));
                if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.State.StartsWith(pager.City));
                if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.StartsWith(pager.State));
                if (!string.IsNullOrWhiteSpace(pager.Zipcode)) pred = pred.And(p => p.Zipcode.StartsWith(pager.Zipcode));
                if (!string.IsNullOrWhiteSpace(pager.Email)) pred = pred.And(p => p.Email.Contains(pager.Email));
                if (!string.IsNullOrWhiteSpace(pager.Phone)) pred = pred.And(p => p.Phone.Contains(pager.Phone));
                if (pager.Source != null) pred = pred.And(p => (int)p.Source == pager.Source);

                var filteredQuery = query.Where(pred);

                var results = await filteredQuery.ProjectTo<DemographicViewModel>().ToListAsync();

                var path = HttpContext.Current.Server.MapPath(@"~\app_data\demographiclist.csv");

                using (var csv = new CsvWriter(new StreamWriter(File.Create(path))))
                {
                    csv.WriteHeader<DemographicChange>();
                    csv.WriteRecords(results);
                }
                var filename = $"demographic-changes-{DateTime.Now:u}.csv";

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
        }
    }
}
