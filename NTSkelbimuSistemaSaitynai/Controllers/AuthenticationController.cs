using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NTSkelbimuSistemaSaitynai.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

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
        [HttpPost]
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

        private async Task<User?> AuthenticateUser(UserLoginDto login)
        {
            return await _context.Users.Where(u => u.Email == login.Email)
                .Where(u => u.Password == login.Password)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Jwt.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim("id", user.IdUser.ToString()),
                new Claim("roles", await GetRoleAsync(user.IdUser)),
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
    }
}

public class RefreshRequestDto
{
    public string RefreshToken { get; set; } = null!;
}
