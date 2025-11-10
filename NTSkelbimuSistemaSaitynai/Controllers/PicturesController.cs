using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages apartment pictures.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class PicturesController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;

        public PicturesController(PostgresContext context, OwnershipService ownershipService)
        {
            _context = context;
            _ownership = ownershipService;
        }

        /// <summary>
        /// Get all pictures.
        /// </summary>
        /// <returns>List of pictures.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Picture>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Picture>>> GetPictures()
        {
            if (User.IsInRole("Administrator"))
            {
                return await _context.Pictures.ToListAsync();
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (currentId == null || !User.IsInRole("Broker"))
            {
                return Forbid();
            }
            var pics = await _context.Pictures
                .Join(_context.Apartments, p => p.FkApartmentidApartment, a => a.IdApartment, (p,a) => new { p,a })
                .Join(_context.Buildings, pa => pa.a.FkBuildingidBuilding, b => b.IdBuilding, (pa,b) => new { pa.p, b })
                .Where(x => x.b.FkBrokeridUser == currentId)
                .Select(x => x.p)
                .ToListAsync();
            return pics;
        }

        /// <summary>
        /// Get picture by ID.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        /// <returns>Picture or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Picture))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Picture>> GetPicture(string id)
        {
            var picture = await _context.Pictures.FindAsync(id);

            if (picture == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            return picture;
        }

        /// <summary>
        /// Update a picture.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        /// <param name="picture">Updated picture payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutPicture(string id, Picture picture)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            // Ensure key matches route
            picture.Id = id;

            _context.Entry(picture).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PictureExists(id))
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
                if (!ApartmentExists(picture.FkApartmentidApartment))
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
        /// Partially update a picture's public flag.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        /// <param name="dto">New public visibility value.</param>
        /// <returns>No content on success, 404 if picture not found.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PatchPicture(string id, [FromBody] PicturePublicPatchDto dto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            var picture = new Picture { Id = id, Public = dto.Public };
            _context.Attach(picture);
            _context.Entry(picture).Property(p => p.Public).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PictureExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new picture.
        /// </summary>
        /// <param name="picture">Picture payload.</param>
        /// <returns>The created picture.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Picture))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Picture>> PostPicture(Picture picture)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsApartment(currentId.Value, picture.FkApartmentidApartment);
                if (!owns)
                {
                    return Forbid();
                }
            }
            _context.Pictures.Add(picture);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PictureExists(picture.Id))
                {
                    return Conflict();
                }
                else if (!ApartmentExists(picture.FkApartmentidApartment))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPicture", new { id = picture.Id }, picture);
        }

        /// <summary>
        /// Delete a picture by ID.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePicture(string id)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            var picture = await _context.Pictures.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            _context.Pictures.Remove(picture);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PictureExists(string id)
        {
            return _context.Pictures.Any(e => e.Id == id);
        }

        private bool ApartmentExists(long id)
        {
            return _context.Apartments.Any(e => e.IdApartment == id);
        }
    }
}
