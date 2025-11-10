using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages viewings of listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    public class ViewingsController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;

        public ViewingsController(PostgresContext context, OwnershipService ownershipService)
        {
            _context = context;
            _ownership = ownershipService;
        }

        /// <summary>
        /// Get all viewings.
        /// </summary>
        /// <returns>List of viewings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Viewing>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Viewing>>> GetViewings()
        {
            if (User.IsInRole("Administrator"))
            {
                return await _context.Viewings.ToListAsync();
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (currentId == null || !User.IsInRole("Broker"))
            {
                return Forbid();
            }
            var viewings = await _context.Viewings
                .Join(_context.Availabilities,
                      v => v.FkAvailabilityidAvailability,
                      a => a.IdAvailability,
                      (v,a) => new { v, a })
                .Where(x => x.a.FkBrokeridUser == currentId)
                .Select(x => x.v)
                .ToListAsync();
            return viewings;
        }

        /// <summary>
        /// Get a viewing by ID.
        /// </summary>
        /// <param name="id">Viewing ID.</param>
        /// <returns>Viewing or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Viewing))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Viewing>> GetViewing(long id)
        {
            var viewing = await _context.Viewings.FindAsync(id);
            if (viewing == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsViewing(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            return viewing;
        }

        /// <summary>
        /// Partially update a viewing's status.
        /// </summary>
        /// <param name="id">Viewing ID.</param>
        /// <param name="dto">Fields to update (currently only status).</param>
        /// <returns>No content on success, 404 if not found, 422 for invalid foreign key.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PatchViewing(long id, [FromBody] ViewingPatchDto dto)
        {
            if (!ViewingExists(id))
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsViewing(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }

            var viewing = new Viewing { IdViewing = id, Status = dto.Status };
            _context.Attach(viewing);
            _context.Entry(viewing).Property(v => v.Status).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!ViewingStatusExists(dto.Status))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Update a viewing.
        /// </summary>
        /// <param name="id">Viewing ID.</param>
        /// <param name="viewingDto">Updated viewing payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutViewing(long id, [FromBody] ViewingDto viewingDto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                // Ownership either by availability owner or by listing owner
                var ownsAvailability = currentId != null && await _ownership.BrokerOwnsAvailability(currentId.Value, viewingDto.FkAvailabilityidAvailability);
                var ownsListing = currentId != null && await _ownership.BrokerOwnsListing(currentId.Value, viewingDto.FkListingidListing);
                if (!(ownsAvailability && ownsListing))
                {
                    return Forbid();
                }
            }
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
                FkListingidListing = viewingDto.FkListingidListing,
            };

            // Set key from route id
            viewing.IdViewing = id;

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
            catch (DbUpdateException)
            {
                if (!AvailabilityExists(viewing.FkAvailabilityidAvailability) || !ListingExists(viewing.FkListingidListing) || !ViewingStatusExists(viewing.Status))
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
        /// Create a new viewing.
        /// </summary>
        /// <param name="viewing">Viewing payload.</param>
        /// <returns>The created viewing.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Viewing))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Viewing>> PostViewing(Viewing viewing)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var ownsAvailability = currentId != null && await _ownership.BrokerOwnsAvailability(currentId.Value, viewing.FkAvailabilityidAvailability);
                var ownsListing = currentId != null && await _ownership.BrokerOwnsListing(currentId.Value, viewing.FkListingidListing);
                if (!(ownsAvailability && ownsListing))
                {
                    return Forbid();
                }
            }
            _context.Viewings.Add(viewing);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!AvailabilityExists(viewing.FkAvailabilityidAvailability) || !ListingExists(viewing.FkListingidListing) || !ViewingStatusExists(viewing.Status))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetViewing", new { id = viewing.IdViewing }, viewing);
        }

        /// <summary>
        /// Delete a viewing by ID.
        /// </summary>
        /// <param name="id">Viewing ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteViewing(long id)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsViewing(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
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

        private bool AvailabilityExists(long id)
        {
            return _context.Availabilities.Any(e => e.IdAvailability == id);
        }

        private bool ListingExists(long id)
        {
            return _context.Listings.Any(e => e.IdListing == id);
        }

        private bool ViewingStatusExists(long id)
        {
            return _context.Viewingstatuses.Any(e => e.IdViewingstatus == id);
        }
    }
}
