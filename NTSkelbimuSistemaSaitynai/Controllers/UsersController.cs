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
    /// <summary>
    /// Manages user resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PostgresContext _context;

        public UsersController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>List of users.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<User>))]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <returns>User or 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Update a user.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="userDto">Updated user payload.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PutUser(long id, UserDto userDto)
        {
            DateTime dt1;

            try
            {
                dt1 = DateTime.Parse(userDto.Registrationtime);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (userDto.Registrationtime.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);

            User user = new User
            {
                Name = userDto.Name,
                Surname = userDto.Surname,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Password = userDto.Password,
                Registrationtime = dt1,
                Profilepicture = userDto.Profilepicture,
            };

            // Assign ID from route so the entity key is set correctly for update
            user.IdUser = id;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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
    /// Create a new user.
    /// </summary>
    /// <param name="userDto">User payload.</param>
    /// <returns>The created user.</returns>
        [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<User>> PostUser([FromBody] UserDto userDto)
        {
            DateTime dt1;

            try
            {
                dt1 = DateTime.Parse(userDto.Registrationtime);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date and time format - expecting yyyy-mm-dd hh:mm");
            }

            if (userDto.Registrationtime.Split(' ').Length < 2)
            {
                return UnprocessableEntity("Invalid date and time format - seems like there is no time value - expecting yyyy-mm-dd hh:mm");
            }

            dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc);

            User user = new User
            {
                Name = userDto.Name,
                Surname = userDto.Surname,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Password = userDto.Password,
                Registrationtime = dt1,
                Profilepicture = userDto.Profilepicture,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.IdUser }, user);
        }

    /// <summary>
    /// Delete a user by ID.
    /// </summary>
    /// <param name="id">User ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.IdUser == id);
        }
    }
}
