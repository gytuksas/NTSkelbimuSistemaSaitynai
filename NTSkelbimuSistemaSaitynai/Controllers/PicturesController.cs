using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using NTSkelbimuSistemaSaitynai.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;
using System;
using System.IO;
using System.Linq;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages apartment pictures.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class PicturesController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;
        private readonly IWebHostEnvironment _environment;

        public PicturesController(PostgresContext context, OwnershipService ownershipService, IWebHostEnvironment environment)
        {
            _context = context;
            _ownership = ownershipService;
            _environment = environment;
        }

        private string EnsureUploadsDirectory()
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            }
            var uploadPath = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadPath);
            return uploadPath;
        }

        private void DeletePictureFile(string pictureId)
        {
            var uploadPath = EnsureUploadsDirectory();
            var filePath = Path.Combine(uploadPath, pictureId);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        /// <summary>
        /// Get all pictures.
        /// </summary>
        /// <returns>List of pictures.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Picture>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Picture>>> GetPictures()
        {
            if (User.IsInRole("Administrator"))
            {
                return await _context.Pictures.ToListAsync();
            }
            var currentId = _ownership.GetCurrentUserId(User);
            if (currentId == null || !User.IsInRole("Broker"))
            {
                return Forbid();
            }
            var pics = await _context.Pictures
                .Join(_context.Apartments, p => p.FkApartmentidApartment, a => a.IdApartment, (p,a) => new { p,a })
                .Join(_context.Buildings, pa => pa.a.FkBuildingidBuilding, b => b.IdBuilding, (pa,b) => new { pa.p, b })
                .Where(x => x.b.FkBrokeridUser == currentId)
                .Select(x => x.p)
                .ToListAsync();
            return pics;
        }

        /// <summary>
        /// Get picture by ID.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        /// <returns>Picture or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Picture))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Picture>> GetPicture(string id)
        {
            var picture = await _context.Pictures.FindAsync(id);

            if (picture == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutPicture(string id, Picture picture)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
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
        /// Partially update a picture's public flag.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        /// <param name="dto">New public visibility value.</param>
        /// <returns>No content on success, 404 if picture not found.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PatchPicture(string id, [FromBody] PicturePublicPatchDto dto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            var picture = new Picture { Id = id, Public = dto.Public };
            _context.Attach(picture);
            _context.Entry(picture).Property(p => p.Public).IsModified = true;

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
                throw;
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Picture>> PostPicture(Picture picture)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsApartment(currentId.Value, picture.FkApartmentidApartment);
                if (!owns)
                {
                    return Forbid();
                }
            }
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
        /// Upload a new picture file and create its record.
        /// </summary>
        /// <param name="request">Upload payload with apartment reference and visibility.</param>
        /// <returns>The stored Picture record.</returns>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Picture))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Picture>> UploadPicture([FromForm] PictureUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("File payload is empty.");
            }

            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsApartment(currentId.Value, request.ApartmentId);
                if (!owns)
                {
                    return Forbid();
                }
            }

            if (!ApartmentExists(request.ApartmentId))
            {
                return UnprocessableEntity("Apartment does not exist.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Palaikomi tik JPG, PNG arba WEBP failai.");
            }

            if (request.File.Length > 10 * 1024 * 1024)
            {
                return BadRequest("Failas per didelis (limitas 10 MB).");
            }

            var uploadsPath = EnsureUploadsDirectory();
            var newFileName = $"{Guid.NewGuid():N}{extension}";
            var destinationPath = Path.Combine(uploadsPath, newFileName);

            await using (var stream = System.IO.File.Create(destinationPath))
            {
                await request.File.CopyToAsync(stream);
            }

            var picture = new Picture
            {
                Id = newFileName,
                Public = request.Public,
                FkApartmentidApartment = request.ApartmentId,
            };

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
                throw;
            }

            return CreatedAtAction(nameof(GetPicture), new { id = picture.Id }, picture);
        }

        /// <summary>
        /// Delete a picture by ID.
        /// </summary>
        /// <param name="id">Picture ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePicture(string id)
        {
            if (!User.IsInRole("Administrator"))
            {
                var currentId = _ownership.GetCurrentUserId(User);
                var owns = currentId != null && await _ownership.BrokerOwnsPicture(currentId.Value, id);
                if (!owns)
                {
                    return Forbid();
                }
            }
            var picture = await _context.Pictures.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            _context.Pictures.Remove(picture);
            await _context.SaveChangesAsync();

            DeletePictureFile(id);

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
