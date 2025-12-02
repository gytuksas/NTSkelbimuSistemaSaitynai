using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NTSkelbimuSistemaSaitynai.Authorization;
using NTSkelbimuSistemaSaitynai.Configuration;
using NTSkelbimuSistemaSaitynai.Models;
using System;
using System.Collections.Generic;
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class PicturesController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly OwnershipService _ownership;
        private readonly IWebHostEnvironment _environment;
        private readonly FileStorageOptions _fileStorageOptions;

        public PicturesController(
            PostgresContext context,
            OwnershipService ownershipService,
            IWebHostEnvironment environment,
            IOptions<FileStorageOptions> fileStorageOptions)
        {
            _context = context;
            _ownership = ownershipService;
            _environment = environment;
            _fileStorageOptions = fileStorageOptions.Value;
        }

        private string EnsureUploadsDirectory()
        {
            var uploadRoot = _fileStorageOptions.UploadRoot;
            if (string.IsNullOrWhiteSpace(uploadRoot))
            {
                uploadRoot = Path.Combine(_environment.ContentRootPath, "storage", "uploads");
            }
            var uploadPath = uploadRoot;
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
        /// <returns>The stored Picture records.</returns>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IEnumerable<Picture>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<Picture>>> UploadPictures([FromForm] PictureUploadRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
            {
                return BadRequest("No files were attached.");
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

            var uploadsPath = EnsureUploadsDirectory();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var createdPictures = new List<Picture>();

            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("One of the files was empty.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Palaikomi tik JPG, PNG arba WEBP failai.");
                }

                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest("Failas per didelis (limitas 10 MB).");
                }

                var picture = new Picture
                {
                    Id = $"{Guid.NewGuid():N}{extension}",
                    Public = request.Public,
                    FkApartmentidApartment = request.ApartmentId,
                };

                var destinationPath = Path.Combine(uploadsPath, picture.Id);

                await using (var stream = System.IO.File.Create(destinationPath))
                {
                    await file.CopyToAsync(stream);
                }

                createdPictures.Add(picture);
            }

            _context.Pictures.AddRange(createdPictures);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                foreach (var pic in createdPictures)
                {
                    DeletePictureFile(pic.Id);
                }

                if (createdPictures.Any(pic => PictureExists(pic.Id)))
                {
                    return Conflict();
                }
                throw;
            }

            return CreatedAtAction(nameof(GetPictures), new { }, createdPictures);
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
