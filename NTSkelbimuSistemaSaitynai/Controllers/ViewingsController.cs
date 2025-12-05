using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;
using System;
using System.Linq;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages viewings of listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        /// Get the authenticated buyer's private viewings with statuses.
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BuyerViewingDto>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<BuyerViewingDto>>> GetMyViewings()
        {
            if (!User.IsInRole("Buyer"))
            {
                return Forbid();
            }

            var currentId = _ownership.GetCurrentUserId(User);
            if (currentId == null)
            {
                return Unauthorized();
            }

            var viewings = await _context.Viewings
                .Where(v => v.FkBuyeridUser == currentId.Value)
                .Join(_context.Listings,
                      v => v.FkListingidListing,
                      l => l.IdListing,
                      (v, l) => new { v, l })
                .Join(_context.Pictures,
                      vl => vl.l.FkPictureid,
                      p => p.Id,
                      (vl, p) => new { vl.v, vl.l, p })
                .Join(_context.Apartments,
                      vlp => vlp.p.FkApartmentidApartment,
                      a => a.IdApartment,
                      (vlp, a) => new { vlp.v, vlp.l, vlp.p, a })
                .Join(_context.Buildings,
                      vlpa => vlpa.a.FkBuildingidBuilding,
                      b => b.IdBuilding,
                      (vlpa, b) => new { vlpa.v, vlpa.l, vlpa.p, vlpa.a, b })
                .Join(_context.Viewingstatuses,
                      vlpab => vlpab.v.Status,
                      s => s.IdViewingstatus,
                      (vlpab, s) => new { vlpab.v, vlpab.l, vlpab.p, vlpab.b, status = s })
                .Join(_context.Users,
                      vlpabs => vlpabs.b.FkBrokeridUser,
                      u => u.IdUser,
                      (vlpabs, u) => new { vlpabs.v, vlpabs.l, vlpabs.p, vlpabs.b, vlpabs.status, broker = u })
                .OrderByDescending(x => x.v.From)
                .Select(x => new BuyerViewingDto
                {
                    Id = x.v.IdViewing,
                    ListingId = x.l.IdListing,
                    ListingTitle = x.l.Description,
                    City = x.b.City,
                    Address = x.b.Address,
                    From = x.v.From,
                    To = x.v.To,
                    StatusId = x.status.IdViewingstatus,
                    Status = x.status.Name,
                    PictureId = x.p.Public ? x.p.Id : null,
                    PictureUrl = null,
                    BrokerName = $"{x.broker.Name} {x.broker.Surname}".Trim(),
                    BrokerPhone = x.broker.Phone
                })
                .ToListAsync();

            return Ok(viewings);
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

            var viewing = await _context.Viewings.FindAsync(id);
            if (viewing == null)
            {
                return NotFound();
            }

            viewing.From = dt1;
            viewing.To = dt2;
            viewing.Status = viewingDto.Status;
            viewing.FkAvailabilityidAvailability = viewingDto.FkAvailabilityidAvailability;
            viewing.FkListingidListing = viewingDto.FkListingidListing;

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
            var availability = await _context.Availabilities.FindAsync(viewing.FkAvailabilityidAvailability);
            if (availability == null)
            {
                return UnprocessableEntity("Availability does not exist");
            }

            var listing = await _context.Listings
                .Include(l => l.FkPicture)
                    .ThenInclude(p => p.FkApartmentidApartmentNavigation)
                        .ThenInclude(a => a.FkBuildingidBuildingNavigation)
                .FirstOrDefaultAsync(l => l.IdListing == viewing.FkListingidListing);

            if (listing == null)
            {
                return UnprocessableEntity("Listing does not exist");
            }

            var listingBrokerId = listing.FkPicture?
                .FkApartmentidApartmentNavigation?
                .FkBuildingidBuildingNavigation?
                .FkBrokeridUser;

            if (listingBrokerId == null)
            {
                return UnprocessableEntity("Listing broker is not configured");
            }

            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (User.IsInRole("Broker"))
                {
                    var ownsAvailability = currentId != null && await _ownership.BrokerOwnsAvailability(currentId.Value, viewing.FkAvailabilityidAvailability);
                    var ownsListing = currentId != null && await _ownership.BrokerOwnsListing(currentId.Value, viewing.FkListingidListing);
                    if (!(ownsAvailability && ownsListing))
                    {
                        return Forbid();
                    }
                }
                else if (User.IsInRole("Buyer"))
                {
                    if (currentId == null)
                    {
                        return Forbid();
                    }

                    if (availability.FkBrokeridUser != listingBrokerId)
                    {
                        return UnprocessableEntity("Availability is not owned by the listing broker");
                    }

                    var slotFrom = DateTime.SpecifyKind(viewing.From, DateTimeKind.Utc);
                    var slotTo = DateTime.SpecifyKind(viewing.To, DateTimeKind.Utc);
                    var slotDuration = slotTo - slotFrom;

                    if (Math.Abs(slotDuration.TotalMinutes - 60) > 0.01)
                    {
                        return UnprocessableEntity("Viewings must reserve exactly one hour.");
                    }

                    var availabilityStart = DateTime.SpecifyKind(availability.From, DateTimeKind.Utc);
                    var availabilityEnd = DateTime.SpecifyKind(availability.To, DateTimeKind.Utc);

                    if (slotFrom < availabilityStart || slotTo > availabilityEnd)
                    {
                        return UnprocessableEntity("Selected time falls outside broker availability.");
                    }

                    var overlaps = await _context.Viewings
                        .AnyAsync(v => v.FkAvailabilityidAvailability == availability.IdAvailability && v.From < slotTo && v.To > slotFrom);

                    if (overlaps)
                    {
                        return UnprocessableEntity("Selected time slot is already booked.");
                    }

                    viewing.From = slotFrom;
                    viewing.To = slotTo;
                    viewing.Status = 1; // Pending confirmation
                    viewing.FkBuyeridUser = currentId.Value;
                }
                else
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
