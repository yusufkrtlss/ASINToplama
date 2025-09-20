using ASINToplama_API.Models;
using ASINToplama_BusinessLayer.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASINToplama_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IConfiguration _cfg;

        public AuthController(IAuthService auth, IConfiguration cfg)
        {
            _auth = auth;
            _cfg = cfg;
        }

        /// <summary> Email + parola ile giriş; başarılıysa JWT access token döner. </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email ve parola zorunludur.");

            var (ok, msg, user) = await _auth.LoginAsync(dto.Email, dto.Password, ct);
            if (!ok || user is null) return Unauthorized(new { message = msg });

            var (token, expiresAtUtc) = CreateAccessToken(user.Id, user.Email, user.IsAdmin);
            return Ok(new
            {
                accessToken = token,
                accessTokenExpiresAt = expiresAtUtc,
                user = new { user.Id, user.Email, user.FullName, user.IsAdmin }
            });
        }

        // JWT üretimi (appsettings: Jwt:Issuer, Audience, SigningKey, AccessTokenMinutes)
        private (string token, DateTime expiresAtUtc) CreateAccessToken(Guid userId, string email, bool isAdmin)
        {
            var jwtSection = _cfg.GetSection("Jwt");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["AccessTokenMinutes"]!));

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User"),
            // İhtiyaca göre: new Claim("scope", "harvest:read harvest:write")
        };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
