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
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public SessionsController(PostgresContext context)
        {
            _context = context;
        }

        // GET: api/Sessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Session>>> GetSessions()
        {
            return await _context.Sessions.ToListAsync();
        }

        // GET: api/Sessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Session>> GetSession(string id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }

        // PUT: api/Sessions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
    public async Task<IActionResult> PutSession(string id, [FromBody] SessionDto sessionDto)
        {
            DateTime dt1;
            DateTime dt2;

            try
            {
                dt1 = DateTime.Parse(sessionDto.Created);
                dt2 = DateTime.Parse(sessionDto.Lastactivity);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (sessionDto.Created.Split(' ').Length < 2 || sessionDto.Lastactivity.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);

            Session session = new Session
            {
                Created = dt1,
                Remember = sessionDto.Remember,
                Lastactivity = dt2,
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

        // POST: api/Sessions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
    public async Task<ActionResult<Session>> PostSession([FromBody] SessionDto sessionDto)
        {
            DateTime dt1;
            DateTime dt2;

            try
            {
                dt1 = DateTime.Parse(sessionDto.Created);
                dt2 = DateTime.Parse(sessionDto.Lastactivity);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (sessionDto.Created.Split(' ').Length < 2 || sessionDto.Lastactivity.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);
            dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);

            Session session = new Session
            {
                Created = dt1,
                Remember = sessionDto.Remember,
                Lastactivity = dt2,
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

        // DELETE: api/Sessions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(string id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
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
