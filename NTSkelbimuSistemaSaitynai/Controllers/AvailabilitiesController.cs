using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;
using NTSkelbimuSistemaSaitynai;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly PostgresContext _context;

        public AvailabilitiesController(PostgresContext context)
        {
            _context = context;
        }

        // GET: api/Availabilities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Availability>>> GetAvailabilities()
        {
            return await _context.Availabilities.ToListAsync();
        }

        // GET: api/Availabilities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Availability>> GetAvailability(long id)
        {
            var availability = await _context.Availabilities.FindAsync(id);

            if (availability == null)
            {
                return NotFound();
            }

            return availability;
        }

        // PUT: api/Availabilities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAvailability(long id, [FromBody] AvailabilityDto availabilityDto)
        {
            DateTime dt1;
            DateTime dt2;

            try
            {
                dt1 = DateTime.Parse(availabilityDto.From);
                dt2 = DateTime.Parse(availabilityDto.To);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (availabilityDto.From.Split(' ').Length < 2 || availabilityDto.To.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);

            Availability availability = new Availability
            {
                From = dt1,
                To = dt2,
                FkBrokeridUser = availabilityDto.FkBrokeridUser
            };

            if (id != availability.IdAvailability)
            {
                return BadRequest();
            }

            _context.Entry(availability).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AvailabilityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException)
            {
                if (!BrokerExists(availability.FkBrokeridUser))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Availabilities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Availability>> PostAvailability(AvailabilityDto availabilityDto)
        {
            DateTime dt1;
            DateTime dt2;

            try
            {
                dt1 = DateTime.Parse(availabilityDto.From);
                dt2 = DateTime.Parse(availabilityDto.To);
            }
            catch(FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (availabilityDto.From.Split(' ').Length < 2 || availabilityDto.To.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);

            Availability availability = new Availability
            {
                From = dt1,
                To = dt2,
                FkBrokeridUser = availabilityDto.FkBrokeridUser
            };

            _context.Availabilities.Add(availability);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!BrokerExists(availability.FkBrokeridUser))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAvailability", new { id = availability.IdAvailability }, availability);
        }

        // DELETE: api/Availabilities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailability(long id)
        {
            var availability = await _context.Availabilities.FindAsync(id);
            if (availability == null)
            {
                return NotFound();
            }

            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AvailabilityExists(long id)
        {
            return _context.Availabilities.Any(e => e.IdAvailability == id);
        }
        private bool BrokerExists(long id)
        {
            return _context.Brokers.Any(e => e.IdUser == id);
        }
    }
}
