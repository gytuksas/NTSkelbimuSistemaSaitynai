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
    /// Manages apartment pictures.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        private readonly PostgresContext _context;

        public PicturesController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all pictures.
        /// </summary>
        /// <returns>List of pictures.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Picture>))]
        public async Task<ActionResult<IEnumerable<Picture>>> GetPictures()
        {
            return await _context.Pictures.ToListAsync();
        }

    /// <summary>
    /// Get picture by ID.
    /// </summary>
    /// <param name="id">Picture ID.</param>
    /// <returns>Picture or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Picture))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Picture>> GetPicture(string id)
        {
            var picture = await _context.Pictures.FindAsync(id);

            if (picture == null)
            {
                return NotFound();
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
        public async Task<IActionResult> PutPicture(string id, Picture picture)
        {
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
    /// Create a new picture.
    /// </summary>
    /// <param name="picture">Picture payload.</param>
    /// <returns>The created picture.</returns>
        [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Picture))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Picture>> PostPicture(Picture picture)
        {
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
        public async Task<IActionResult> DeletePicture(string id)
        {
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
