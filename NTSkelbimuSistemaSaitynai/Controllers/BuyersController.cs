using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages buyer resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BuyersController : ControllerBase
    {
        private readonly PostgresContext _context;

        public BuyersController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all buyers.
        /// </summary>
        /// <returns>List of buyers.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Buyer>))]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetBuyers()
        {
            return await _context.Buyers.ToListAsync();
        }

        /// <summary>
        /// Get a buyer by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>Buyer or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Buyer))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Buyer>> GetBuyer(long id)
        {
            var buyer = await _context.Buyers.FindAsync(id);

            if (buyer == null)
            {
                return NotFound();
            }

            return buyer;
        }

        /// <summary>
        /// Partially update a buyer's status.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="dto">One of confirmed or blocked must be provided.</param>
        /// <returns>No content on success, 400 for invalid payload, 404 if not found.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchBuyer(long id, [FromBody] BuyerPatchDto dto)
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

            if (!BuyerExists(id))
            {
                return NotFound();
            }

            var buyer = new Buyer { IdUser = id };
            _context.Attach(buyer);

            if (dto.Confirmed.HasValue)
            {
                buyer.Confirmed = dto.Confirmed.Value;
                _context.Entry(buyer).Property(b => b.Confirmed).IsModified = true;
            }
            else
            {
                buyer.Blocked = dto.Blocked!.Value;
                _context.Entry(buyer).Property(b => b.Blocked).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Update a buyer.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="buyer">Updated buyer payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutBuyer(long id, Buyer buyer)
        {
            buyer.IdUser = id;

            _context.Entry(buyer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuyerExists(id))
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
        /// Create a new buyer.
        /// </summary>
        /// <param name="buyer">Buyer payload.</param>
        /// <returns>The created buyer.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Buyer))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Buyer>> PostBuyer(Buyer buyer)
        {
            _context.Buyers.Add(buyer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BuyerExists(buyer.IdUser))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBuyer", new { id = buyer.IdUser }, buyer);
        }

        /// <summary>
        /// Delete a buyer by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBuyer(long id)
        {
            var buyer = await _context.Buyers.FindAsync(id);
            if (buyer == null)
            {
                return NotFound();
            }

            _context.Buyers.Remove(buyer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BuyerExists(long id)
        {
            return _context.Buyers.Any(e => e.IdUser == id);
        }
    }
}
