using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DonorGateway.Admin.Helpers;
using DonorGateway.Admin.ViewModels;
using DonorGateway.Data;
using DonorGateway.Domain;

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/constituent")]
    public class ConstituentController : ApiController
    {
        private readonly DataContext _context;
        private const int PAGE_SIZE = 20;

        public ConstituentController()
        {
            _context = DataContext.Create();
        }

        [Route("{lookupId}")]
        public async Task<object> Get(string lookupId)
        {
            var constituent = await _context.Constituents.SingleOrDefaultAsync(c => c.LookupId == lookupId);
            if (constituent == null) return NotFound();

            var model = Mapper.Map<ConstituentViewModel>(constituent);
            return Ok(model);
        }

        public async Task<object> Get([FromUri]ConsituentSearchViewModel pager)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Restart();
                if (pager == null) pager = new ConsituentSearchViewModel();

                var query = _context.Constituents;
                var totalCount = await query.CountAsync();

                var pred = PredicateBuilder.True<Constituent>();
                if (!string.IsNullOrWhiteSpace(pager.Name)) pred = pred.And(p => p.Name.Contains(pager.Name));
                if (!string.IsNullOrWhiteSpace(pager.FinderNumber)) pred = pred.And(p => p.FinderNumber.Contains(pager.FinderNumber));
                if (!string.IsNullOrWhiteSpace(pager.LookupId)) pred = pred.And(p => p.LookupId.Contains(pager.LookupId));
                if (!string.IsNullOrWhiteSpace(pager.City)) pred = pred.And(p => p.State.StartsWith(pager.City));
                if (!string.IsNullOrWhiteSpace(pager.State)) pred = pred.And(p => p.State.StartsWith(pager.State));
                if (!string.IsNullOrWhiteSpace(pager.Zipcode)) pred = pred.And(p => p.Zipcode.StartsWith(pager.Zipcode));
                if (!string.IsNullOrWhiteSpace(pager.Email)) pred = pred.And(p => p.Email.Contains(pager.Email));
                if (!string.IsNullOrWhiteSpace(pager.Phone)) pred = pred.And(p => p.Phone.Contains(pager.Phone));

                var filteredQuery = query.Where(pred);
                var pagerCount = filteredQuery.Count();
                var totalPages = Math.Ceiling((double)pagerCount / pager.PageSize ?? PAGE_SIZE);

                var results = await filteredQuery.Where(pred)
                    .Order(pager.OrderBy, pager.OrderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending)
                    .Skip(pager.PageSize * (pager.Page - 1) ?? 0)
                    .Take(pager.PageSize ?? PAGE_SIZE)
                    .ProjectTo<ConstituentViewModel>().ToListAsync();

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

        public async Task<object> Put(ConstituentViewModel vm)
        {
            if (vm.Id == 0) return NotFound();

            var c = await _context.Constituents.FindAsync(vm.Id);
            if (c == null) return NotFound();


            //var demoChange = Mapper.Map<DemographicChange>(vm);

            var demoChange = new DemographicChange
            {
                Name = vm.Name,
                FinderNumber = vm.FinderNumber,
                LookupId = vm.LookupId,
                Street = vm.Street,
                Street2 = vm.Street2,
                City = vm.City,
                State = vm.State,
                Zipcode = vm.Zipcode,
                Email = vm.Email,
                Phone = vm.Phone,
                Source = Source.Tax
            };
            _context.DemographicChanges.Add(demoChange);

            var constituent = Mapper.Map<Constituent>(vm);
            _context.Constituents.AddOrUpdate(constituent);
            _context.SaveChanges();

            Mapper.Map(constituent, vm);

            return Ok(vm);
        }


    }
}
