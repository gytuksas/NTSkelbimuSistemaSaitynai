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
    /// <summary>
    /// Manages property listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public ListingsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all listings.
        /// </summary>
        /// <returns>List of listings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Listing>))]
        public async Task<ActionResult<IEnumerable<Listing>>> GetListings()
        {
            return await _context.Listings.ToListAsync();
        }

    /// <summary>
    /// Get a listing by ID.
    /// </summary>
    /// <param name="id">Listing ID.</param>
    /// <returns>Listing or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Listing))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Listing>> GetListing(long id)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing == null)
            {
                return NotFound();
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
        public async Task<IActionResult> PutListing(long id, Listing listing)
        {
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
        public async Task<ActionResult<Listing>> PostListing(Listing listing)
        {
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
        public async Task<IActionResult> DeleteListing(long id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
            {
                return NotFound();
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
