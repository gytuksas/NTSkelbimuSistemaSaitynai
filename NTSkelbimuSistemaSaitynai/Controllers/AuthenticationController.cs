using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NTSkelbimuSistemaSaitynai.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Linq;
using NTSkelbimuSistemaSaitynai.Security;

namespace NTSkelbimuSistemaSaitynai.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly Configuration.Configuration _config;
        private readonly PostgresContext _context;

        public AuthenticationController(PostgresContext context)
        {
            _config = Configuration.Configuration.GetConfiguration();
            _context = context;
        }

        /// <summary>
        /// Allows user to log onto the system.
        /// </summary>
        /// <param name="login">Username and password</param>
        /// <returns>JWT token or error</returns>
    [AllowAnonymous]
    [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Login([FromBody] UserLoginDto login)
        {
            User? user = await AuthenticateUser(login);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!await IsUserConfirmedAsync(user.IdUser))
            {
                Response.Headers["X-Account-Unconfirmed"] = "true";
                return Unauthorized("Account is pending confirmation by the administrator");
            }

            if (await IsUserBlockedAsync(user.IdUser))
            {
                Response.Headers["X-Account-Blocked"] = "true";
                await RevokeActiveSessionsAsync(user.IdUser);
                return Unauthorized("Account is blocked by the administrator");
            }

            var accessToken = await GenerateJSONWebToken(user);
            // Create refresh token session record
            var refreshToken = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            Session session = new Session
            {
                Id = refreshToken,
                Created = now,
                Lastactivity = now,
                Remember = false,
                Expires = now.AddDays(7), // 7 day refresh lifetime
                Revoked = false,
                FkUseridUser = user.IdUser
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { accessToken, refreshToken });
        }

        /// <summary>
        /// Registers a new buyer.
        /// </summary>
        /// <param name="request">Buyer registration payload.</param>
        /// <returns>Created buyer identity.</returns>
        [AllowAnonymous]
        [HttpPost("register/buyer")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegistrationResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public Task<ActionResult<RegistrationResponseDto>> RegisterBuyer([FromBody] RegistrationRequestDto request)
        {
            return RegisterUserAsync(request, userId =>
            {
                _context.Buyers.Add(new Buyer
                {
                    IdUser = userId,
                    Confirmed = false,
                    Blocked = false
                });
                return Task.CompletedTask;
            }, "Buyer");
        }

        /// <summary>
        /// Registers a new broker.
        /// </summary>
        /// <param name="request">Broker registration payload.</param>
        /// <returns>Created broker identity.</returns>
        [AllowAnonymous]
        [HttpPost("register/broker")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegistrationResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public Task<ActionResult<RegistrationResponseDto>> RegisterBroker([FromBody] RegistrationRequestDto request)
        {
            return RegisterUserAsync(request, userId =>
            {
                _context.Brokers.Add(new Broker
                {
                    IdUser = userId,
                    Confirmed = false,
                    Blocked = false
                });
                return Task.CompletedTask;
            }, "Broker");
        }

        /// <summary>
        /// Exchanges a valid (non-revoked, unexpired) refresh token for a new access token and refresh token.
        /// </summary>
        /// <param name="request">Refresh token payload</param>
        /// <returns>New access and refresh tokens</returns>
        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("RefreshToken is required");
            }

            var existing = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == request.RefreshToken);
            if (existing == null)
            {
                return Unauthorized("Invalid refresh token");
            }
            if (existing.Revoked || existing.Expires <= DateTime.UtcNow)
            {
                return Unauthorized("Refresh token expired or revoked");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == existing.FkUseridUser);
            if (user == null)
            {
                return Unauthorized("Associated user not found");
            }

            if (!await IsUserConfirmedAsync(user.IdUser))
            {
                existing.Revoked = true;
                Response.Headers["X-Account-Unconfirmed"] = "true";
                await _context.SaveChangesAsync();
                return Unauthorized("Account is pending confirmation by the administrator");
            }

            if (await IsUserBlockedAsync(user.IdUser))
            {
                existing.Revoked = true;
                Response.Headers["X-Account-Blocked"] = "true";
                await RevokeActiveSessionsAsync(user.IdUser, saveChanges: false);
                await _context.SaveChangesAsync();
                return Unauthorized("Account is blocked by the administrator");
            }

            // Revoke old token
            existing.Revoked = true;
            existing.Lastactivity = DateTime.UtcNow;

            // Issue new session (refresh token)
            var newRefreshToken = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            Session newSession = new Session
            {
                Id = newRefreshToken,
                Created = now,
                Lastactivity = now,
                Remember = false,
                Expires = now.AddDays(7),
                Revoked = false,
                FkUseridUser = user.IdUser
            };
            _context.Sessions.Add(newSession);

            var newAccessToken = await GenerateJSONWebToken(user);

            await _context.SaveChangesAsync();
            return Ok(new {accessToken = newAccessToken, refreshToken = newRefreshToken });
        }

        /// <summary>
        /// Logs out a user by revoking the provided refresh token and instructing the client to remove access token.
        /// </summary>
        /// <param name="request">Refresh token to revoke.</param>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("RefreshToken is required");
            }

            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == request.RefreshToken);
            if (session == null)
            {
                return Unauthorized();
            }

            // Ensure the refresh belongs to current user (or admin)
            var userIdStr = User.FindFirst("id")?.Value;
            long.TryParse(userIdStr, out var userId);
            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && session.FkUseridUser != userId)
            {
                return Forbid();
            }

            session.Revoked = true;
            session.Lastactivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // If tokens are in cookies, expire them; otherwise client must delete storage
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            Response.Headers.Append("Clear-Authorization", "true");

            return NoContent();
        }

        private async Task<User?> AuthenticateUser(UserLoginDto login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null)
            {
                return null;
            }
            // Verify hashed password
            if (PasswordHasher.Verify(login.Password, user.Password))
            {
                return user;
            }
            return null;
        }

        private async Task<ActionResult<RegistrationResponseDto>> RegisterUserAsync(
            RegistrationRequestDto request,
            Func<long, Task> attachRole,
            string role)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                return Conflict("Nurodytas el. paštas jau naudojamas.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var user = new User
                {
                    Name = request.Name.Trim(),
                    Surname = request.Surname.Trim(),
                    Email = normalizedEmail,
                    Phone = request.Phone.Trim(),
                    Password = PasswordHasher.IsHashed(request.Password)
                        ? request.Password
                        : PasswordHasher.Hash(request.Password),
                    Registrationtime = now,
                    Profilepicture = null
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await attachRole(user.IdUser);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Created($"/api/Users/{user.IdUser}", new RegistrationResponseDto
                {
                    UserId = user.IdUser,
                    Role = role
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string> GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Jwt.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var role = await GetRoleAsync(user.IdUser);
            var claims = new[]
            {
                new Claim("id", user.IdUser.ToString()),
                new Claim("role", role),
                new Claim(ClaimTypes.Role, role),
            };

            var token = new JwtSecurityToken(_config.Jwt.Issuer, //issuer
                _config.Jwt.Issuer, //audience
                claims, //claims
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GetRoleAsync(long userId)
        {
            if (await _context.Administrators.AnyAsync(a => a.IdUser == userId))
                return "Administrator";
            if (await _context.Brokers.AnyAsync(b => b.IdUser == userId))
                return "Broker";
            if (await _context.Buyers.AnyAsync(bu => bu.IdUser == userId))
                return "Buyer";
            return "User";
        }

        private async Task<bool> IsUserBlockedAsync(long userId)
        {
            var broker = await _context.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.IdUser == userId);
            if (broker?.Blocked == true)
            {
                return true;
            }

            var buyer = await _context.Buyers.AsNoTracking().FirstOrDefaultAsync(b => b.IdUser == userId);
            return buyer?.Blocked == true;
        }

        private async Task<bool> IsUserConfirmedAsync(long userId)
        {
            if (await _context.Administrators.AsNoTracking().AnyAsync(a => a.IdUser == userId))
            {
                return true;
            }

            var broker = await _context.Brokers.AsNoTracking()
                .Where(b => b.IdUser == userId)
                .Select(b => new { b.Confirmed })
                .FirstOrDefaultAsync();
            if (broker != null)
            {
                return broker.Confirmed;
            }

            var buyer = await _context.Buyers.AsNoTracking()
                .Where(b => b.IdUser == userId)
                .Select(b => new { b.Confirmed })
                .FirstOrDefaultAsync();
            if (buyer != null)
            {
                return buyer.Confirmed;
            }

            // If the user does not belong to a role requiring confirmation, treat them as confirmed.
            return true;
        }

        private async Task RevokeActiveSessionsAsync(long userId, bool saveChanges = true)
        {
            var sessions = await _context.Sessions
                .Where(s => s.FkUseridUser == userId && !s.Revoked && s.Expires > DateTime.UtcNow)
                .ToListAsync();

            if (!sessions.Any())
            {
                return;
            }

            foreach (var session in sessions)
            {
                session.Revoked = true;
                session.Lastactivity = DateTime.UtcNow;
            }

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}

public class RefreshRequestDto
{
    public string RefreshToken { get; set; } = null!;
}

public class RegistrationRequestDto
{
    [Required]
    [StringLength(128, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 2)]
    public string Surname { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    public string Phone { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}

public class RegistrationResponseDto
{
    public long UserId { get; set; }
    public string Role { get; set; } = null!;
}
