using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages broker resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BrokersController : ControllerBase
    {
        private readonly PostgresContext _context;

        public BrokersController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all brokers.
        /// </summary>
        /// <returns>List of brokers.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Broker>))]
        public async Task<ActionResult<IEnumerable<Broker>>> GetBrokers()
        {
            return await _context.Brokers.ToListAsync();
        }

        /// <summary>
        /// Get a broker by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>Broker or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Broker))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Broker>> GetBroker(long id)
        {
            var broker = await _context.Brokers.FindAsync(id);

            if (broker == null)
            {
                return NotFound();
            }

            return broker;
        }

        [HttpGet("listings/{id}")]
        public async Task<ActionResult<Listing>> GetBrokerListings(long id)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }

            var listings = await _context.Buildings
                .Where(b => b.FkBrokeridUser == id)
                .SelectMany(b => b.Apartments)
                .SelectMany(a => a.Pictures)
                .Select(p => p.Listing)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(listings);
        }

        [HttpGet("apartments/{id}")]
        public async Task<ActionResult<Apartment>> GetBrokerApartments(long id)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }

            var apartments = await _context.Buildings
                .Where(b => b.FkBrokeridUser == id)
                .SelectMany(b => b.Apartments)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(apartments);
        }

        [HttpGet("viewings/{id}")]
        public async Task<ActionResult<IEnumerable<Viewing>>> GetViewingsForBroker(long id)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }

            var viewings = await _context.Availabilities
                .Where(a => a.FkBrokeridUser == id)
                .SelectMany(b => b.Viewings)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(viewings);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchBroker(long id, [FromBody] BrokerPatchDto dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            var setCount = (dto.Confirmed.HasValue ? 1 : 0) + (dto.Blocked.HasValue ? 1 : 0);
            if (setCount != 1)
            {
                return BadRequest("Provide exactly one of: confirmed or blocked.");
            }

            if (!BrokerExists(id))
            {
                return NotFound();
            }

            var broker = new Broker { IdUser = id };
            _context.Attach(broker);

            if (dto.Confirmed.HasValue)
            {
                broker.Confirmed = dto.Confirmed.Value;
                _context.Entry(broker).Property(b => b.Confirmed).IsModified = true;
            }
            else
            {
                broker.Blocked = dto.Blocked!.Value;
                _context.Entry(broker).Property(b => b.Blocked).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Update a broker.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="broker">Updated broker payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutBroker(long id, Broker broker)
        {
            broker.IdUser = id;

            _context.Entry(broker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrokerExists(id))
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

        /// <summary>
        /// Create a new broker.
        /// </summary>
        /// <param name="broker">Broker payload.</param>
        /// <returns>The created broker.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Broker))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Broker>> PostBroker(Broker broker)
        {
            _context.Brokers.Add(broker);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BrokerExists(broker.IdUser))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBroker", new { id = broker.IdUser }, broker);
        }

        /// <summary>
        /// Delete a broker by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBroker(long id)
        {
            var broker = await _context.Brokers.FindAsync(id);
            if (broker == null)
            {
                return NotFound();
            }

            _context.Brokers.Remove(broker);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BrokerExists(long id)
        {
            return _context.Brokers.Any(e => e.IdUser == id);
        }
    }
}
