using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages viewings of listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ViewingsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public ViewingsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all viewings.
        /// </summary>
        /// <returns>List of viewings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Viewing>))]
        public async Task<ActionResult<IEnumerable<Viewing>>> GetViewings()
        {
            return await _context.Viewings.ToListAsync();
        }

        /// <summary>
        /// Get a viewing by ID.
        /// </summary>
        /// <param name="id">Viewing ID.</param>
        /// <returns>Viewing or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Viewing))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Viewing>> GetViewing(long id)
        {
            var viewing = await _context.Viewings.FindAsync(id);

            if (viewing == null)
            {
                return NotFound();
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
        public async Task<IActionResult> PatchViewing(long id, [FromBody] ViewingPatchDto dto)
        {
            if (!ViewingExists(id))
            {
                return NotFound();
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
        public async Task<ActionResult<Viewing>> PostViewing(Viewing viewing)
        {
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
