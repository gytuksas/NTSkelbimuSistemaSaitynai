using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewingsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public ViewingsController(PostgresContext context)
        {
            _context = context;
        }

        // GET: api/Viewings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Viewing>>> GetViewings()
        {
            return await _context.Viewings.ToListAsync();
        }

        // GET: api/Viewings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Viewing>> GetViewing(long id)
        {
            var viewing = await _context.Viewings.FindAsync(id);

            if (viewing == null)
            {
                return NotFound();
            }

            return viewing;
        }

        // PUT: api/Viewings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutViewing(long id, [FromBody] ViewingDto viewingDto)
        {
            DateTime dt1;
            DateTime dt2;

            try
            {
                dt1 = DateTime.Parse(viewingDto.From);
                dt2 = DateTime.Parse(viewingDto.To);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (viewingDto.From.Split(' ').Length < 2 || viewingDto.To.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);

            Viewing viewing = new Viewing
            {
                From = dt1,
                To = dt2,
                Status = viewingDto.Status,
                FkAvailabilityidAvailability = viewingDto.FkAvailabilityidAvailability,
            };

            if (id != viewing.IdViewing)
            {
                return BadRequest();
            }

            _context.Entry(viewing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ViewingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Viewings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Viewing>> PostViewing(Viewing viewing)
        {
            _context.Viewings.Add(viewing);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetViewing", new { id = viewing.IdViewing }, viewing);
        }

        // DELETE: api/Viewings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteViewing(long id)
        {
            var viewing = await _context.Viewings.FindAsync(id);
            if (viewing == null)
            {
                return NotFound();
            }

            _context.Viewings.Remove(viewing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ViewingExists(long id)
        {
            return _context.Viewings.Any(e => e.IdViewing == id);
        }
    }
}
