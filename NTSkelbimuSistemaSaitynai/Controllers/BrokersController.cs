using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;
using System.Runtime.CompilerServices;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages broker resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class BrokersController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;

        public BrokersController(PostgresContext context, OwnershipService ownershipService)
        {
            _context = context;
            _ownership = ownershipService;
        }

        /// <summary>
        /// Get all brokers.
        /// </summary>
        /// <returns>List of brokers.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Broker>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Broker>>> GetBrokers()
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            return await _context.Brokers.ToListAsync();
        }

        /// <summary>
        /// Get a broker by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>Broker or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Broker))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Broker>> GetBroker(long id)
        {
            var broker = await _context.Brokers.FindAsync(id);
            if (broker == null)
            {
                return NotFound();
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }
            return broker;
        }

        /// <summary>
        /// Get all listings managed by a broker.
        /// </summary>
        /// <param name="id">Broker's user ID.</param>
        /// <returns>List of listings or 404 if broker not found.</returns>
        [HttpGet("{id}/listings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Listing>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Listing>>> GetBrokerListings(long id)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var listings = await _context.Buildings
                .Where(b => b.FkBrokeridUser == id)
                .SelectMany(b => b.Apartments)
                .SelectMany(a => a.Pictures)
                .Select(p => p.Listing)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(listings);
        }

        /// <summary>
        /// Get all apartments managed by a broker.
        /// </summary>
        /// <param name="id">Broker's user ID.</param>
        /// <returns>List of apartments or 404 if broker not found.</returns>
        [HttpGet("{id}/apartments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Apartment>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetBrokerApartments(long id)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var apartments = await _context.Buildings
                .Where(b => b.FkBrokeridUser == id)
                .SelectMany(b => b.Apartments)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(apartments);
        }

    /// <summary>
    /// Get apartments for a specific building managed by a broker.
    /// </summary>
    /// <param name="id">Broker's user ID.</param>
    /// <param name="buildingId">Building ID.</param>
    /// <returns>List of apartments or 404 if not found.</returns>
    [HttpGet("{id}/building/{buildingId}/apartments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Apartment>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetBrokerApartmentsInBuilding(long id, long buildingId)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }
            if (!BuildingExists(buildingId))
            {
                return NotFound("No building with this ID");
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var apartments = await _context.Buildings
                .Where(b => b.FkBrokeridUser == id)
                .Where(b => b.IdBuilding == buildingId)
                .SelectMany(a => a.Apartments)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(apartments);
        }

        /// <summary>
        /// Get all viewings scheduled for a broker.
        /// </summary>
        /// <param name="id">Broker's user ID.</param>
        /// <returns>List of viewings or 404 if broker not found.</returns>
        [HttpGet("{id}/viewings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Viewing>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Viewing>>> GetViewingsForBroker(long id)
        {
            if (!BrokerExists(id))
            {
                return NotFound("No broker with this ID");
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var viewings = await _context.Availabilities
                .Where(a => a.FkBrokeridUser == id)
                .SelectMany(b => b.Viewings)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(viewings);
        }

    /// <summary>
    /// Get all viewings scheduled in a specific availability slot for a broker.
    /// </summary>
    /// <param name="id">Broker's user ID.</param>
    /// <param name="availabilityId">Availability ID.</param>
    /// <returns>List of viewings or 404 if not found.</returns>
    [HttpGet("{id}/availability/{availabilityId}/viewings")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Viewing>>> GetBrokerViewingsInAvailability(long id, long availabilityId)
        {
            if (!BrokerExists(id))
                return NotFound("No broker with this ID");
            if (!AvailabilityExists(availabilityId))
                return NotFound("No availability with this ID");
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var viewings = await _context.Availabilities
                .Where(a => a.FkBrokeridUser == id)
                .Where(a => a.IdAvailability == availabilityId)
                .SelectMany(b => b.Viewings)
                .Where(l => l != null)
                .ToListAsync();

            return Ok(viewings);
        }

    /// <summary>
    /// Get all listings for an apartment within a building managed by a broker.
    /// </summary>
    /// <param name="id">Broker's user ID.</param>
    /// <param name="buildingId">Building ID.</param>
    /// <param name="apartmentId">Apartment ID.</param>
    /// <returns>List of listings or 404 if not found.</returns>
    [HttpGet("{id}/building/{buildingId}/apartment/{apartmentId}/listings")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Listing>>> GetListingsForBuildingApartment(long id, long buildingId, long apartmentId)
        {
            if (!BrokerExists(id))
                return NotFound("No broker with this ID");
            if (!BuildingExists(buildingId))
                return NotFound("No building with this ID");
            if (!ApartmentExists(apartmentId))
                return NotFound("No apartment with this ID");
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var listings = await _context.Buildings
            .Where(b => b.FkBrokeridUser == id)
            .Where(b => b.IdBuilding == buildingId)
            .SelectMany(a => a.Apartments)
            .Where(a => a.IdApartment == apartmentId)
            .SelectMany(p => p.Pictures)
            .Select(l => l.Listing)
            .Where(l => l != null)
            .ToListAsync();

            return Ok(listings);
        }

        //see if a specific picture, belonging to a specific apartment in a specific building is being used in a listing
    /// <summary>
    /// Check if a specific picture (within an apartment and building) is used in any listing.
    /// </summary>
    /// <param name="id">Broker's user ID.</param>
    /// <param name="buildingId">Building ID.</param>
    /// <param name="apartmentId">Apartment ID.</param>
    /// <param name="pictureId">Picture ID.</param>
    /// <returns>List of listings that use the picture.</returns>
    [HttpGet("{id}/building/{buildingId}/apartment/{apartmentId}/picture/{pictureId}/listing")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Listing>>> GetListingsWherePictureAppears(long id, long buildingId, long apartmentId, string pictureId)
        {
            if (!BrokerExists(id))
                return NotFound("No broker with this ID");
            if (!BuildingExists(buildingId))
                return NotFound("No building with this ID");
            if (!ApartmentExists(apartmentId))
                return NotFound("No apartment with this ID");
            if (!PictureExists(pictureId))
                return NotFound("No picture with this ID");
            var currentId = _ownership.GetCurrentUserId(User);
            if (!(User.IsInRole("Administrator") || (User.IsInRole("Broker") && currentId == id)))
            {
                return Forbid();
            }

            var listings = await _context.Buildings
            .Where(b => b.FkBrokeridUser == id)
            .Where(b => b.IdBuilding == buildingId)
            .SelectMany(a => a.Apartments)
            .Where(a => a.IdApartment == apartmentId)
            .SelectMany(p => p.Pictures)
            .Where(p => p.Id == pictureId)
            .Select(l => l.Listing)
            .Where(l => l != null)
            .ToListAsync();

            return Ok(listings);
        }


        /// <summary>
        /// Partially update a broker's status.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="dto">One of confirmed or blocked must be provided.</param>
        /// <returns>No content on success, 400 for invalid payload, 404 if not found.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PatchBroker(long id, [FromBody] BrokerPatchDto dto)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            if (dto == null)
            {
                return BadRequest();
            }

            var setCount = (dto.Confirmed.HasValue ? 1 : 0) + (dto.Blocked.HasValue ? 1 : 0);
            if (setCount != 1)
            {
                return BadRequest("Provide exactly one of: confirmed or blocked.");
            }

            if (!BrokerExists(id))
            {
                return NotFound();
            }

            var broker = new Broker { IdUser = id };
            _context.Attach(broker);

            if (dto.Confirmed.HasValue)
            {
                broker.Confirmed = dto.Confirmed.Value;
                _context.Entry(broker).Property(b => b.Confirmed).IsModified = true;
            }
            else
            {
                broker.Blocked = dto.Blocked!.Value;
                _context.Entry(broker).Property(b => b.Blocked).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Update a broker.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="broker">Updated broker payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutBroker(long id, Broker broker)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            broker.IdUser = id;

            _context.Entry(broker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrokerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new broker.
        /// </summary>
        /// <param name="broker">Broker payload.</param>
        /// <returns>The created broker.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Broker))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Broker>> PostBroker(Broker broker)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            _context.Brokers.Add(broker);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BrokerExists(broker.IdUser))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBroker", new { id = broker.IdUser }, broker);
        }

        /// <summary>
        /// Delete a broker by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteBroker(long id)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            var broker = await _context.Brokers.FindAsync(id);
            if (broker == null)
            {
                return NotFound();
            }

            _context.Brokers.Remove(broker);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BrokerExists(long id)
        {
            return _context.Brokers.Any(e => e.IdUser == id);
        }
        private bool BuildingExists(long id)
        {
            return _context.Buildings.Any(e => e.IdBuilding == id);
        }
        private bool ApartmentExists(long id)
        {
            return _context.Apartments.Any(e => e.IdApartment == id);
        }
        private bool PictureExists(string id)
        {
            return _context.Pictures.Any(e => e.Id == id);
        }
        private bool AvailabilityExists(long id)
        {
            return _context.Availabilities.Any(e => e.IdAvailability == id);
        }
    }
}
