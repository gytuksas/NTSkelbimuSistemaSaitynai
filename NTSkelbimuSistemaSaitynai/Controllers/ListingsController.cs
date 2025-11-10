using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages property listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class ListingsController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;

        public ListingsController(PostgresContext context, OwnershipService ownershipService)
        {
            _context = context;
            _ownership = ownershipService;
        }

        /// <summary>
        /// Get all listings.
        /// </summary>
        /// <returns>List of listings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Listing>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Listing>>> GetListings()
        {
            if (User.IsInRole("Administrator"))
            {
                return await _context.Listings.ToListAsync();
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (currentId == null || !User.IsInRole("Broker"))
            {
                return Forbid();
            }
            var listings = await _context.Listings
                .Where(l => true) // anchor
                .Join(_context.Pictures, l => l.FkPictureid, p => p.Id, (l,p) => new { l, p })
                .Join(_context.Apartments, lp => lp.p.FkApartmentidApartment, a => a.IdApartment, (lp,a) => new { lp.l, a })
                .Join(_context.Buildings, la => la.a.FkBuildingidBuilding, b => b.IdBuilding, (la,b) => new { la.l, b })
                .Where(x => x.b.FkBrokeridUser == currentId)
                .Select(x => x.l)
                .ToListAsync();
            return listings;
        }

        /// <summary>
        /// Get a listing by ID.
        /// </summary>
        /// <param name="id">Listing ID.</param>
        /// <returns>Listing or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Listing))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Listing>> GetListing(long id)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsListing(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            return listing;
        }

        /// <summary>
        /// Update a listing.
        /// </summary>
        /// <param name="id">Listing ID.</param>
        /// <param name="listing">Updated listing payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutListing(long id, Listing listing)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsListing(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            listing.IdListing = id;

            _context.Entry(listing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ListingExists(id))
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
                if (listing.FkPictureid != null && !PictureExists(listing.FkPictureid))
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
        /// Create a new listing.
        /// </summary>
        /// <param name="listing">Listing payload.</param>
        /// <returns>The created listing.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Listing))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Listing>> PostListing(Listing listing)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var ownsPicture = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, listing.FkPictureid);
                if (!ownsPicture)
                {
                    return Forbid();
                }
            }
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetListing", new { id = listing.IdListing }, listing);
        }

        /// <summary>
        /// Delete a listing by ID.
        /// </summary>
        /// <param name="id">Listing ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteListing(long id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsListing(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }

            _context.Listings.Remove(listing);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (listing.FkPictureid != null && !PictureExists(listing.FkPictureid))
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

        private bool ListingExists(long id)
        {
            return _context.Listings.Any(e => e.IdListing == id);
        }

        private bool PictureExists(string id)
        {
            return _context.Pictures.Any(e => e.Id == id);
        }
    }
}
