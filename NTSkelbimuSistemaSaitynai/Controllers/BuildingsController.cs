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
    /// Manages building resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public BuildingsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all buildings.
        /// </summary>
        /// <returns>List of buildings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Building>))]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuildings()
        {
            return await _context.Buildings.ToListAsync();
        }

    /// <summary>
    /// Get a building by ID.
    /// </summary>
    /// <param name="id">Building ID.</param>
    /// <returns>Building or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Building>> GetBuilding(long id)
        {
            var building = await _context.Buildings.FindAsync(id);

            if (building == null)
            {
                return NotFound();
            }

            return building;
        }

    /// <summary>
    /// Update a building.
    /// </summary>
    /// <param name="id">Building ID.</param>
    /// <param name="building">Updated building payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PutBuilding(long id, Building building)
        {
            building.IdBuilding = id;

            _context.Entry(building).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuildingExists(id))
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
                if (!BrokerExists(building.FkBrokeridUser))
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
    /// Create a new building.
    /// </summary>
    /// <param name="building">Building payload.</param>
    /// <returns>The created building.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Building>> PostBuilding(Building building)
        {
            _context.Buildings.Add(building);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!BrokerExists(building.FkBrokeridUser))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBuilding", new { id = building.IdBuilding }, building);
        }

    /// <summary>
    /// Delete a building by ID.
    /// </summary>
    /// <param name="id">Building ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBuilding(long id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BuildingExists(long id)
        {
            return _context.Buildings.Any(e => e.IdBuilding == id);
        }
        private bool BrokerExists(long id)
        {
            return _context.Brokers.Any(e => e.IdUser == id);
        }
    }
}
