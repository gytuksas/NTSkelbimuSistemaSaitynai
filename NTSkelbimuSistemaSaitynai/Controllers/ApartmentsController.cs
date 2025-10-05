using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;
using NTSkelbimuSistemaSaitynai;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages apartment resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public ApartmentsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all apartments.
        /// </summary>
        /// <returns>List of apartments.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Apartment>))]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartments()
        {
            return await _context.Apartments.ToListAsync();
        }

    /// <summary>
    /// Get an apartment by ID.
    /// </summary>
    /// <param name="id">Apartment ID.</param>
    /// <returns>Apartment resource or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Apartment))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Apartment>> GetApartment(long id)
        {
            var apartment = await _context.Apartments.FindAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            return apartment;
        }

    /// <summary>
    /// Get apartments in a specific building.
    /// </summary>
    /// <param name="id">Building ID.</param>
    /// <returns>List of apartments in a building or 404.</returns>
    [HttpGet("building/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Apartment>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartmentsInBuilding(long id)
        {
            var apartments = await _context.
                Apartments.
                Where(x => x.FkBuildingidBuilding == id).
                ToListAsync();

            if(apartments == null)
                return NotFound();

            return apartments;
        }

    /// <summary>
    /// Update an apartment.
    /// </summary>
    /// <param name="id">Apartment ID to update.</param>
    /// <param name="apartment">Updated apartment payload.</param>
        [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PutApartment(long id, Apartment apartment)
        {
            // Ensure ID is aligned with route
            apartment.IdApartment = id;

            _context.Entry(apartment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApartmentExists(id))
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
                if(!BuildingExists(apartment.FkBuildingidBuilding))
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
    /// Create a new apartment.
    /// </summary>
    /// <param name="apartment">Apartment payload.</param>
    /// <returns>The created apartment.</returns>
        [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Apartment))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Apartment>> PostApartment(Apartment apartment)
        {
            _context.Apartments.Add(apartment);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!BuildingExists(apartment.FkBuildingidBuilding))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetApartment", new { id = apartment.IdApartment }, apartment);
        }

    /// <summary>
    /// Delete an apartment by ID.
    /// </summary>
    /// <param name="id">Apartment ID to delete.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteApartment(long id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApartmentExists(long id)
        {
            return _context.Apartments.Any(e => e.IdApartment == id);
        }

        private bool BuildingExists(long id)
        {
            return _context.Buildings.Any(e => e.IdBuilding == id);
        }
    }
}
