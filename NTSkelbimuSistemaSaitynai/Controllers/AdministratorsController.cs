using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages administrator resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdministratorsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public AdministratorsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all administrators.
        /// </summary>
        /// <returns>List of administrators.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Administrator>))]
        public async Task<ActionResult<IEnumerable<Administrator>>> GetAdministrators()
        {
            return await _context.Administrators.ToListAsync();
        }

        /// <summary>
        /// Get an administrator by ID.
        /// </summary>
        /// <param name="id">User ID of the administrator.</param>
        /// <returns>Administrator resource or 404 if not found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Administrator))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Administrator>> GetAdministrator(long id)
        {
            var administrator = await _context.Administrators.FindAsync(id);

            if (administrator == null)
            {
                return NotFound();
            }

            return administrator;
        }

        /// <summary>
        /// Update an administrator.
        /// </summary>
        /// <param name="id">User ID of the administrator to update.</param>
        /// <param name="administrator">Updated administrator payload.</param>
        /// <remarks>Returns 404 if the resource does not exist.</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAdministrator(long id, Administrator administrator)
        {
            administrator.IdUser = id;

            _context.Entry(administrator).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdministratorExists(id))
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
        /// Create a new administrator.
        /// </summary>
        /// <param name="administrator">Administrator payload.</param>
        /// <returns>The created administrator.</returns>
        /// <remarks>Returns 409 if the administrator already exists.</remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Administrator))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Administrator>> PostAdministrator(Administrator administrator)
        {
            _context.Administrators.Add(administrator);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AdministratorExists(administrator.IdUser))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAdministrator", new { id = administrator.IdUser }, administrator);
        }

        /// <summary>
        /// Delete an administrator by ID.
        /// </summary>
        /// <param name="id">User ID of the administrator to delete.</param>
        /// <remarks>Returns 404 if the resource does not exist.</remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAdministrator(long id)
        {
            var administrator = await _context.Administrators.FindAsync(id);
            if (administrator == null)
            {
                return NotFound();
            }

            _context.Administrators.Remove(administrator);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdministratorExists(long id)
        {
            return _context.Administrators.Any(e => e.IdUser == id);
        }
    }
}
