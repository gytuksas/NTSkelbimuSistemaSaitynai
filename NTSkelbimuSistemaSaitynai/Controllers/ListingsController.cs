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
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
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
                BrokerName = broker != null ? $"{broker.Name} {broker.Surname}" : null,
                BrokerPhone = broker?.Phone,
                GalleryPictureIds = gallery,
                GalleryPictureUrls = gallery
                    .Select(id => ResolvePictureUrl(id))
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url!)
                    .ToList(),
                Availabilities = availabilities
            };

            details.PictureUrl = ResolvePictureUrl(details.PictureId);

            return Ok(details);
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
