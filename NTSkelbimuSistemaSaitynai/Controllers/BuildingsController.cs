using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages building resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    public class BuildingsController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;

        public BuildingsController(PostgresContext context, OwnershipService ownershipService)
        {
            _context = context;
            _ownership = ownershipService;
        }

        /// <summary>
        /// Get all buildings.
        /// </summary>
        /// <returns>List of buildings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Building>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuildings()
        {
            if (User.IsInRole("Administrator"))
            {
                return await _context.Buildings.ToListAsync();
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (currentId == null || !User.IsInRole("Broker"))
            {
                return Forbid();
            }
            var buildings = await _context.Buildings.Where(b => b.FkBrokeridUser == currentId).ToListAsync();
            return buildings;
        }

        /// <summary>
        /// Get a building by ID.
        /// </summary>
        /// <param name="id">Building ID.</param>
        /// <returns>Building or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Building>> GetBuilding(long id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (currentId == null || building.FkBrokeridUser != currentId.Value)
                {
                    return Forbid();
                }
            }
            return building;
        }

    /// <summary>
    /// Get apartments in a specific building.
    /// </summary>
    /// <param name="id">Building ID.</param>
    /// <returns>List of apartments in a building or 404.</returns>
    [HttpGet("{id}/apartments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Apartment>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartmentsInBuilding(long id)
        {
            var apartments = await _context.
                Apartments.
                Where(x => x.FkBuildingidBuilding == id).
                ToListAsync();

            if (apartments == null)
                return NotFound();
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsBuilding(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            return apartments;
        }

    /// <summary>
    /// Get all pictures in a specific building.
    /// </summary>
    /// <param name="id">Building ID.</param>
    /// <returns>List of pictures in the building or 404.</returns>
    [HttpGet("{id}/pictures")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Picture>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Picture>>> GetPicturesInBuilding(long id)
        {
            if (!BuildingExists(id))
            {
                return NotFound("No building with provided id");
            }

            var pictures = await _context.
                Apartments.
                Where(x => x.FkBuildingidBuilding == id)
                .SelectMany(p => p.Pictures)
                .Where(l => l != null)
                .ToListAsync();

            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsBuilding(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            return pictures;
        }

        /// <summary>
        /// Get a specific picture located in a specific apartment within a building.
        /// </summary>
        /// <param name="id">Building ID.</param>
        /// <param name="apartmentId">Apartment ID within the building.</param>
        /// <param name="pictureId">Picture ID to fetch.</param>
        /// <returns>The picture if found; otherwise 404.</returns>
        [HttpGet("{id}/apartment/{apartmentId}/picture/{pictureId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Picture))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Picture>> GetPictureInBuildingApartment(long id, long apartmentId, string pictureId)
        {
            if (!BuildingExists(id))
                return NotFound("No building with provided id");
            if (!ApartmentExists(apartmentId))
                return NotFound("No apartment with provided id");
            if (!PictureExists(pictureId))
                return NotFound("No picture with provided id");

            var picture = await _context.
                Apartments.
                Where(a => a.FkBuildingidBuilding == id)
                .Where(a => a.IdApartment == apartmentId)
                .SelectMany(p => p.Pictures)
                .Where(p => p.Id == pictureId)
                .Where(l => l != null)
                .SingleOrDefaultAsync();

            if (picture == null)
                return NotFound("No picture with provided IDs");
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var ownsApartment = currentId != null && await _ownership.BrokerOwnsApartment(currentId.Value, apartmentId);
                if (!ownsApartment)
                {
                    return Forbid();
                }
            }
            return picture;
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutBuilding(long id, Building building)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (currentId == null || building.FkBrokeridUser != currentId.Value)
                {
                    return Forbid();
                }
            }
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
                if (!BrokerExists(building.FkBrokeridUser)
                    || (building.Energy.HasValue && !EnergyclassExists(building.Energy.Value)))
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Building>> PostBuilding(Building building)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (currentId == null || building.FkBrokeridUser != currentId.Value)
                {
                    return Forbid();
                }
            }
            _context.Buildings.Add(building);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!BrokerExists(building.FkBrokeridUser)
                    || (building.Energy.HasValue && !EnergyclassExists(building.Energy.Value)))
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteBuilding(long id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (currentId == null || building.FkBrokeridUser != currentId.Value)
                {
                    return Forbid();
                }
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

        private bool EnergyclassExists(int id)
        {
            return _context.Energyclasses.Any(e => e.IdEnergyclass == id);
        }
        private bool ApartmentExists(long id)
        {
            return _context.Apartments.Any(e => e.IdApartment == id);
        }
        private bool PictureExists(string id)
        {
            return _context.Pictures.Any(e => e.Id == id);
        }
    }
}
