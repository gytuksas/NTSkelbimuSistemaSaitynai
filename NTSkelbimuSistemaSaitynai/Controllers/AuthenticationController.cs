using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NTSkelbimuSistemaSaitynai.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

            var tokenString = GenerateJSONToken(user);
            return Ok(new { token = tokenString });
        }

        private async Task<User?> AuthenticateUser(UserLoginDto login)
        {
            return await _context.Users.Where(u => u.Email == login.Email)
                .Where(u => u.Password == login.Password)
                .FirstOrDefaultAsync();
        }

        private string GenerateJSONToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Jwt.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config.Jwt.Issuer, //issuer
                _config.Jwt.Issuer, //audience
                null, //claims
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
