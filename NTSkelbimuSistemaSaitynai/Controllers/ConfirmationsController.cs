using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages buyer email/identity confirmations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    public class ConfirmationsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public ConfirmationsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all confirmations.
        /// </summary>
        /// <returns>List of confirmations.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Confirmation>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Confirmation>>> GetConfirmations()
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            return await _context.Confirmations.ToListAsync();
        }

        /// <summary>
        /// Get a confirmation by ID.
        /// </summary>
        /// <param name="id">Confirmation ID.</param>
        /// <returns>Confirmation or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Confirmation))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Confirmation>> GetConfirmation(string id)
        {
            var confirmation = await _context.Confirmations.FindAsync(id);

            if (confirmation == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (confirmation.FkBuyeridUser != uid)
                {
                    return Forbid();
                }
            }
            return confirmation;
        }

        /// <summary>
        /// Update a confirmation.
        /// </summary>
        /// <param name="id">Confirmation ID.</param>
        /// <param name="confirmationDto">Updated confirmation payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutConfirmation(string id, [FromBody] ConfirmationDto confirmationDto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (confirmationDto.FkBuyeridUser != uid)
                {
                    return Forbid();
                }
            }
            DateTime dt1;

            try
            {
                dt1 = DateTime.Parse(confirmationDto.Expires);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (confirmationDto.Expires.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);

            Confirmation confirmation = new Confirmation
            {
                Expires = dt1,
                FkBuyeridUser = confirmationDto.FkBuyeridUser
            };

            // Assign ID from route
            confirmation.Id = id;

            _context.Entry(confirmation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConfirmationExists(id))
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
                if (!BuyerExists(confirmation.FkBuyeridUser))
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

        /// <summary>
        /// Create a new confirmation.
        /// </summary>
        /// <param name="confirmationDto">Confirmation payload.</param>
        /// <returns>The created confirmation.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Confirmation))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Confirmation>> PostConfirmation([FromBody] ConfirmationDto confirmationDto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (confirmationDto.FkBuyeridUser != uid)
                {
                    return Forbid();
                }
            }
            DateTime dt1;

            try
            {
                dt1 = DateTime.Parse(confirmationDto.Expires);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (confirmationDto.Expires.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);

            Confirmation confirmation = new Confirmation
            {
                Expires = dt1,
                FkBuyeridUser = confirmationDto.FkBuyeridUser
            };

            // Generate an ID if not provided (PK cannot be null)
            if (string.IsNullOrWhiteSpace(confirmation.Id))
            {
                confirmation.Id = Guid.NewGuid().ToString();
            }

            _context.Confirmations.Add(confirmation);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ConfirmationExists(confirmation.Id))
                {
                    return Conflict();
                }
                else if (!BuyerExists(confirmation.FkBuyeridUser))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }


            return CreatedAtAction("GetConfirmation", new { id = confirmation.Id }, confirmation);
        }

        /// <summary>
        /// Delete a confirmation by ID.
        /// </summary>
        /// <param name="id">Confirmation ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteConfirmation(string id)
        {
            var confirmation = await _context.Confirmations.FindAsync(id);
            if (confirmation == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (confirmation.FkBuyeridUser != uid)
                {
                    return Forbid();
                }
            }

            _context.Confirmations.Remove(confirmation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConfirmationExists(string id)
        {
            return _context.Confirmations.Any(e => e.Id == id);
        }

        private bool BuyerExists(long id)
        {
            return _context.Buyers.Any(e => e.IdUser == id);
        }
    }
}
