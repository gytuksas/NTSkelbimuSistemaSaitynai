using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    /// <summary>
    /// Manages user sessions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NTSkelbimuSistemaSaitynai.Authorization.NotBlockedFilter))]
    public class SessionsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public SessionsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all sessions.
        /// </summary>
        /// <returns>List of sessions.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Session>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<Session>>> GetSessions()
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            return await _context.Sessions.ToListAsync();
        }

        /// <summary>
        /// Get a session by ID.
        /// </summary>
        /// <param name="id">Session ID.</param>
        /// <returns>Session or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Session))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Session>> GetSession(string id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (session.FkUseridUser != uid)
                {
                    return Forbid();
                }
            }
            return session;
        }

        /// <summary>
        /// Update a session.
        /// </summary>
        /// <param name="id">Session ID.</param>
        /// <param name="sessionDto">Updated session payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PutSession(string id, [FromBody] SessionDto sessionDto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (sessionDto.FkUseridUser != uid)
                {
                    return Forbid();
                }
            }
            DateTime dt1;
            DateTime dt2;
            DateTime dt3;

            try
            {
                dt1 = DateTime.Parse(sessionDto.Created);
                dt2 = DateTime.Parse(sessionDto.Lastactivity);
                dt3 = DateTime.Parse(sessionDto.Expires ?? sessionDto.Lastactivity); // fallback
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (sessionDto.Created.Split(' ').Length < 2 || sessionDto.Lastactivity.Split(' ').Length < 2 || (sessionDto.Expires != null && sessionDto.Expires.Split(' ').Length < 2))
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);
            dt3 = DateTime.SpecifyKind(dt3, DateTimeKind.Utc);

            Session session = new Session
            {
                Created = dt1,
                Remember = sessionDto.Remember,
                Lastactivity = dt2,
                Expires = dt3,
                Revoked = sessionDto.Revoked ?? false,
                FkUseridUser = sessionDto.FkUseridUser
            };

            // Assign ID from route to properly track the entity
            session.Id = id;

            _context.Entry(session).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionExists(id))
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
                if (!UserExists(session.FkUseridUser))
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
        /// Create a new session.
        /// </summary>
        /// <param name="sessionDto">Session payload.</param>
        /// <returns>The created session.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Session))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Session>> PostSession([FromBody] SessionDto sessionDto)
        {
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (sessionDto.FkUseridUser != uid)
                {
                    return Forbid();
                }
            }
            DateTime dt1;
            DateTime dt2;
            DateTime dt3;

            try
            {
                dt1 = DateTime.Parse(sessionDto.Created);
                dt2 = DateTime.Parse(sessionDto.Lastactivity);
                dt3 = DateTime.Parse(sessionDto.Expires ?? sessionDto.Lastactivity);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (sessionDto.Created.Split(' ').Length < 2 || sessionDto.Lastactivity.Split(' ').Length < 2 || (sessionDto.Expires != null && sessionDto.Expires.Split(' ').Length < 2))
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);
            dt3 = DateTime.SpecifyKind(dt3, DateTimeKind.Utc);

            Session session = new Session
            {
                Created = dt1,
                Remember = sessionDto.Remember,
                Lastactivity = dt2,
                Expires = dt3,
                Revoked = sessionDto.Revoked ?? false,
                FkUseridUser = sessionDto.FkUseridUser
            };

            // Ensure we generate a unique ID if not provided by client (PK cannot be null)
            if (string.IsNullOrWhiteSpace(session.Id))
            {
                session.Id = Guid.NewGuid().ToString();
            }

            _context.Sessions.Add(session);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SessionExists(session.Id))
                {
                    return Conflict();
                }
                else if (!UserExists(session.FkUseridUser))
                {
                    return UnprocessableEntity("Invalid fk");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSession", new { id = session.Id }, session);
        }

        /// <summary>
        /// Delete a session by ID.
        /// </summary>
        /// <param name="id">Session ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteSession(string id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                var uidStr = User.FindFirst("id")?.Value;
                long.TryParse(uidStr, out var uid);
                if (session.FkUseridUser != uid)
                {
                    return Forbid();
                }
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SessionExists(string id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.IdUser == id);
        }
    }
}
