using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DonorGateway.Admin.ViewModels;
using DonorGateway.Data;
using DonorGateway.Domain;

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/tax")]
    public class TaxController : ApiController
    {
        private readonly DataContext _context;

        public TaxController()
        {
            _context = DataContext.Create();
        }

        [Route("{lookupId}")]
        public async Task<object> Get(string lookupId)
        {
            var constituent = await _context.Constituents.FirstOrDefaultAsync(e => e.LookupId == lookupId);

            if (constituent == null) return NotFound();

            var taxes = await _context.TaxItems.Where(e => e.ConstituentId == constituent.Id).ProjectTo<TaxItemViewModel>().ToListAsync();

            return Ok(taxes);
        }

        public async Task<object> Put([FromBody] TaxItemViewModel model)
        {
            var tax = await _context.TaxItems.FindAsync(model.Id);
            if (tax == null) return BadRequest();

            var taxItem = Mapper.Map<TaxItem>(model);
            _context.TaxItems.AddOrUpdate(taxItem);
            await _context.SaveChangesAsync();

            Mapper.Map(taxItem, model);
            return Ok(model);
        }

        public async Task<object> Post([FromBody] TaxItemViewModel model)
        {
            var taxItem = Mapper.Map<TaxItem>(model);
            _context.TaxItems.AddOrUpdate(taxItem);
            await _context.SaveChangesAsync();

            Mapper.Map(taxItem, model);
            return Ok(model);
        }

        [Route("{id:int}")]
        public async Task<object> Delete(int id)
        {
            var taxItem = await _context.TaxItems.FindAsync(id);
            if (taxItem == null) return BadRequest();

            _context.TaxItems.Remove(taxItem);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
