using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using System.Web.Http;
using DonorGateway.Data;
using DonorGateway.Domain;

namespace DonorGateway.Admin.Controllers
{
    [RoutePrefix("api/template")]
    public class TemplateController : ApiController
    {
        private readonly DataContext _context;

        public TemplateController()
        {
            _context = DataContext.Create();
        }

        public async Task<object> Get(int id)
        {
            try
            {
                var template = await _context.Templates.SingleOrDefaultAsync(e => e.Id == id);
                if (template == null) return BadRequest("No template found");

                return Ok(template);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public async Task<object> Put(Template model)
        {
            try
            {
                var template = await _context.Templates.SingleOrDefaultAsync(e => e.Id == model.Id);
                if (template == null) return BadRequest("Template not found");

                _context.Templates.AddOrUpdate(model);
                await _context.SaveChangesAsync();
                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
