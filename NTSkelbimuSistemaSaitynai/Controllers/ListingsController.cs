using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NTSkelbimuSistemaSaitynai.Configuration;
using NTSkelbimuSistemaSaitynai.Models;
using System.IO;
using System.Linq;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages property listings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class ListingsController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;
        private readonly FileStorageOptions _fileStorageOptions;

        public ListingsController(PostgresContext context, OwnershipService ownershipService, IOptions<FileStorageOptions> fileStorageOptions)
        {
            _context = context;
            _ownership = ownershipService;
            _fileStorageOptions = fileStorageOptions.Value;
        }

        /// <summary>
        /// Public listings feed intended for guests (limited information, no contacts).
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PublicListingDto>))]
        public async Task<ActionResult<IEnumerable<PublicListingDto>>> GetPublicListings()
        {
            var listingsQuery = from listing in _context.Listings.Include(l => l.Viewing)
                                let viewing = listing.Viewing
                                join picture in _context.Pictures on listing.FkPictureid equals picture.Id into pictureGroup
                                from picture in pictureGroup.DefaultIfEmpty()
                                join apartment in _context.Apartments on picture.FkApartmentidApartment equals apartment.IdApartment into apartmentGroup
                                from apartment in apartmentGroup.DefaultIfEmpty()
                                join building in _context.Buildings on apartment.FkBuildingidBuilding equals building.IdBuilding into buildingGroup
                                from building in buildingGroup.DefaultIfEmpty()
                                select new PublicListingDto
                                {
                                    Id = listing.IdListing,
                                    Description = listing.Description,
                                    AskingPrice = listing.Askingprice,
                                    Rent = listing.Rent,
                                    PictureId = picture != null && picture.Public ? picture.Id : null,
                                    BuildingCity = building != null ? building.City : null,
                                    BuildingAddress = building != null ? building.Address : null,
                                    NextViewingFrom = viewing != null ? viewing.From : (DateTime?)null,
                                    NextViewingTo = viewing != null ? viewing.To : (DateTime?)null,
                                };

            var listings = await listingsQuery.ToListAsync();
            foreach (var listing in listings)
            {
                listing.PictureUrl = ResolvePictureUrl(listing.PictureId);
            }
            return Ok(listings);
        }

        /// <summary>
        /// Public listing details with basic apartment and broker info.
        /// </summary>
        [HttpGet("public/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublicListingDetailsDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PublicListingDetailsDto>> GetPublicListing(long id)
        {
            var listing = await _context.Listings
                .Include(l => l.Viewing)
                .Include(l => l.FkPicture)
                    .ThenInclude(p => p.FkApartmentidApartmentNavigation)
                        .ThenInclude(a => a.FkBuildingidBuildingNavigation)
                            .ThenInclude(b => b.EnergyNavigation)
                .Include(l => l.FkPicture)
                    .ThenInclude(p => p.FkApartmentidApartmentNavigation)
                        .ThenInclude(a => a.FinishNavigation)
                .Include(l => l.FkPicture)
                    .ThenInclude(p => p.FkApartmentidApartmentNavigation)
                        .ThenInclude(a => a.HeatingNavigation)
                .FirstOrDefaultAsync(l => l.IdListing == id);

            if (listing == null)
            {
                return NotFound();
            }

            var picture = listing.FkPicture;
            var apartment = picture?.FkApartmentidApartmentNavigation;
            var building = apartment?.FkBuildingidBuildingNavigation;

            User? broker = null;
            if (building != null)
            {
                broker = await _context.Users.FindAsync(building.FkBrokeridUser);
            }

            var gallery = apartment == null
                ? new List<string>()
                : await _context.Pictures
                    .Where(p => p.FkApartmentidApartment == apartment.IdApartment && p.Public)
                    .Select(p => p.Id)
                    .ToListAsync();

            var brokerId = building?.FkBrokeridUser;
            var availabilities = brokerId == null
                ? new List<PublicAvailabilityDto>()
                : await _context.Availabilities
                    .Where(a => a.FkBrokeridUser == brokerId && a.To >= DateTime.UtcNow)
                    .OrderBy(a => a.From)
                    .Select(a => new PublicAvailabilityDto
                    {
                        Id = a.IdAvailability,
                        From = a.From,
                        To = a.To
                    })
                    .ToListAsync();

            var availableSlots = await BuildAvailableSlotsAsync(availabilities);
            var publicViewings = await BuildPublicViewingsAsync(listing.IdListing);

            var details = new PublicListingDetailsDto
            {
                Id = listing.IdListing,
                Description = listing.Description,
                AskingPrice = listing.Askingprice,
                Rent = listing.Rent,
                PictureId = picture != null && picture.Public ? picture.Id : null,
                BuildingCity = building?.City,
                BuildingAddress = building?.Address,
                NextViewingFrom = listing.Viewing?.From,
                NextViewingTo = listing.Viewing?.To,
                ApartmentArea = apartment?.Area,
                Rooms = apartment?.Rooms,
                ApartmentId = apartment?.IdApartment,
                BuildingId = building?.IdBuilding,
                BuildingEnergyClass = building?.EnergyNavigation?.Name,
                BuildingFloors = building?.Floors,
                BuildingYear = building?.Year,
                BuildingLastRenovationYear = building?.Lastrenovationyear,
                ApartmentNumber = apartment?.Apartmentnumber,
                ApartmentFloor = apartment?.Iswholebuilding == true ? null : apartment?.Floor,
                ApartmentIsWholeBuilding = apartment?.Iswholebuilding,
                ApartmentNotes = string.IsNullOrWhiteSpace(apartment?.Notes) ? null : apartment?.Notes,
                ApartmentHeating = apartment?.HeatingNavigation?.Name,
                ApartmentFinish = apartment?.FinishNavigation?.Name,
                BrokerName = broker != null ? $"{broker.Name} {broker.Surname}" : null,
                BrokerPhone = broker?.Phone,
                GalleryPictureIds = gallery,
                GalleryPictureUrls = gallery
                    .Select(id => ResolvePictureUrl(id))
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url!)
                    .ToList(),
                Availabilities = availabilities,
                AvailableSlots = availableSlots,
                PublicViewings = publicViewings
            };

            details.PictureUrl = ResolvePictureUrl(details.PictureId);

            return Ok(details);
        }

        /// <summary>
        /// Create or update a public viewing for a listing owned by the broker.
        /// </summary>
        [HttpPost("{id}/public-viewing")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublicViewingDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PublicViewingDto>> UpsertPublicViewing(long id, [FromBody] PublicViewingRequest request)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (currentId == null || !await _ownership.BrokerOwnsListing(currentId.Value, id))
                {
                    return Forbid();
                }
            }

            if (request == null)
            {
                return BadRequest("Missing request payload.");
            }

            DateTime from;
            DateTime to;

            try
            {
                from = DateTime.Parse(request.From);
                to = DateTime.Parse(request.To);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (request.From.Split(' ').Length < 2 || request.To.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            if (from >= to)
            {
                return UnprocessableEntity("Pabaiga turi būti vėliau nei pradžia.");
            }

            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var listing = await _context.Listings
                .Include(l => l.Viewing)
                .FirstOrDefaultAsync(l => l.IdListing == id);

            if (listing == null)
            {
                return NotFound();
            }

            var publicStatusId = await ResolvePublicViewingStatusIdAsync();
            if (!publicStatusId.HasValue)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Public viewing status is not configured.");
            }

            Viewing viewing;
            if (listing.Viewing != null)
            {
                viewing = listing.Viewing;
                viewing.From = from;
                viewing.To = to;
                viewing.Status = publicStatusId.Value;

                var availability = await _context.Availabilities.FirstOrDefaultAsync(a => a.IdAvailability == viewing.FkAvailabilityidAvailability);
                if (availability != null)
                {
                    availability.From = from;
                    availability.To = to;
                }
            }
            else
            {
                var brokerId = await ResolveListingBrokerIdAsync(id);
                if (!brokerId.HasValue)
                {
                    return UnprocessableEntity("Listing broker is not configured.");
                }

                var availability = new Availability
                {
                    From = from,
                    To = to,
                    FkBrokeridUser = brokerId.Value
                };

                viewing = new Viewing
                {
                    From = from,
                    To = to,
                    Status = publicStatusId.Value,
                    FkListingidListing = id,
                    FkAvailabilityidAvailabilityNavigation = availability
                };

                availability.Viewings.Add(viewing);
                _context.Availabilities.Add(availability);
                _context.Viewings.Add(viewing);
                listing.Viewing = viewing;
            }

            await _context.SaveChangesAsync();

            return Ok(new PublicViewingDto
            {
                Id = viewing.IdViewing,
                From = viewing.From,
                To = viewing.To
            });
        }

        /// <summary>
        /// Delete an existing public viewing for a listing.
        /// </summary>
        [HttpDelete("{id}/public-viewing")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePublicViewing(long id)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                if (currentId == null || !await _ownership.BrokerOwnsListing(currentId.Value, id))
                {
                    return Forbid();
                }
            }

            var listing = await _context.Listings
                .Include(l => l.Viewing)
                .FirstOrDefaultAsync(l => l.IdListing == id);

            if (listing == null)
            {
                return NotFound();
            }

            if (listing.Viewing == null)
            {
                return NotFound();
            }

            var viewing = listing.Viewing;
            var availability = await _context.Availabilities
                .Include(a => a.Viewings)
                .FirstOrDefaultAsync(a => a.IdAvailability == viewing.FkAvailabilityidAvailability);

            _context.Viewings.Remove(viewing);

            if (availability != null && availability.Viewings.Count <= 1)
            {
                _context.Availabilities.Remove(availability);
            }

            await _context.SaveChangesAsync();

            return NoContent();
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

        private async Task<List<PublicAvailabilitySlotDto>> BuildAvailableSlotsAsync(List<PublicAvailabilityDto> availabilities)
        {
            var slots = new List<PublicAvailabilitySlotDto>();
            if (availabilities == null || availabilities.Count == 0)
            {
                return slots;
            }

            var availabilityIds = availabilities.Select(a => a.Id).ToList();
            var existingViewings = await _context.Viewings
                .Where(v => availabilityIds.Contains(v.FkAvailabilityidAvailability))
                .Select(v => new { v.FkAvailabilityidAvailability, v.From, v.To })
                .ToListAsync();

            var now = DateTime.UtcNow;

            foreach (var availability in availabilities)
            {
                var availabilityStart = DateTime.SpecifyKind(availability.From, DateTimeKind.Utc);
                var availabilityEnd = DateTime.SpecifyKind(availability.To, DateTimeKind.Utc);
                var slotStart = availabilityStart;

                while (slotStart.AddHours(1) <= availabilityEnd)
                {
                    if (slotStart < now)
                    {
                        slotStart = slotStart.AddHours(1);
                        continue;
                    }

                    var slotEnd = slotStart.AddHours(1);
                    var hasOverlap = existingViewings.Any(v => v.FkAvailabilityidAvailability == availability.Id && v.From < slotEnd && v.To > slotStart);

                    if (!hasOverlap)
                    {
                        slots.Add(new PublicAvailabilitySlotDto
                        {
                            AvailabilityId = availability.Id,
                            From = slotStart,
                            To = slotEnd
                        });
                    }

                    slotStart = slotStart.AddHours(1);
                }
            }

            return slots
                .OrderBy(s => s.From)
                .ToList();
        }

        private async Task<List<PublicViewingDto>> BuildPublicViewingsAsync(long listingId)
        {
            var publicStatusId = await ResolvePublicViewingStatusIdAsync();

            if (!publicStatusId.HasValue)
            {
                return new List<PublicViewingDto>();
            }

            var now = DateTime.UtcNow;

            return await _context.Viewings
                .Where(v => v.FkListingidListing == listingId && v.Status == publicStatusId.Value && v.To >= now)
                .OrderBy(v => v.From)
                .Select(v => new PublicViewingDto
                {
                    Id = v.IdViewing,
                    From = v.From,
                    To = v.To
                })
                .ToListAsync();
        }

        private async Task<int?> ResolvePublicViewingStatusIdAsync()
        {
            return await _context.Viewingstatuses
                .Where(status => status.Name.ToLower() == "public")
                .Select(status => (int?)status.IdViewingstatus)
                .FirstOrDefaultAsync();
        }

        private async Task<long?> ResolveListingBrokerIdAsync(long listingId)
        {
            return await _context.Listings
                .Where(l => l.IdListing == listingId)
                .Join(_context.Pictures,
                      l => l.FkPictureid,
                      p => p.Id,
                      (l, p) => new { l, p })
                .Join(_context.Apartments,
                      lp => lp.p.FkApartmentidApartment,
                      a => a.IdApartment,
                      (lp, a) => new { lp.l, a })
                .Join(_context.Buildings,
                      la => la.a.FkBuildingidBuilding,
                      b => b.IdBuilding,
                      (la, b) => b)
                .Select(b => (long?)b.FkBrokeridUser)
                .FirstOrDefaultAsync();
        }

        private bool ListingExists(long id)
        {
            return _context.Listings.Any(e => e.IdListing == id);
        }

        private bool PictureExists(string id)
        {
            return _context.Pictures.Any(e => e.Id == id);
        }

        private string NormalizeRequestPath(string? requestPath)
        {
            var normalized = string.IsNullOrWhiteSpace(requestPath) ? "/uploads" : requestPath.Trim();
            if (!normalized.StartsWith('/'))
            {
                normalized = "/" + normalized.TrimStart('/');
            }
            return normalized.TrimEnd('/');
        }

        private string? ResolvePictureUrl(string? pictureId)
        {
            if (string.IsNullOrWhiteSpace(pictureId))
            {
                return null;
            }

            var fileName = pictureId;
            var candidatePath = Path.Combine(_fileStorageOptions.UploadRoot, pictureId);
            if (!System.IO.File.Exists(candidatePath))
            {
                var fallback = Directory
                    .EnumerateFiles(_fileStorageOptions.UploadRoot, $"{pictureId}.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
                if (fallback != null)
                {
                    fileName = Path.GetFileName(fallback);
                }
            }

            var requestPath = NormalizeRequestPath(_fileStorageOptions.RequestPath);
            return $"{requestPath}/{fileName}";
        }
    }
}
