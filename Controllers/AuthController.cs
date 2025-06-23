using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Api.Models;
using NotesApp.Api.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
//using Internal;

namespace NotesApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        //[HttpGet("ping")]
        //[AllowAnonymous]
        //public IActionResult Ping() => Ok("pong");
        [HttpGet("envtest")]
        [AllowAnonymous]
        public IActionResult EnvTest()
        {
            var jwtSecret = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
                return BadRequest("Jwt:Secret is missing!");
            return Ok($"Jwt:Secret is {jwtSecret.Length} chars long.");
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest("User already exists with this email.");
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userService.CreateAsync(newUser);
            return Ok("User registered successfully.");
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userService.GetByEmailAsync(request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized("Invalid email or password.");
                }

                var jwtSecret = _configuration["Jwt:Secret"];
                if (string.IsNullOrWhiteSpace(jwtSecret))
                {
                    throw new Exception("JWT Secret is missing.");
                }

                var token = GenerateJwtToken(user, jwtSecret);


                return Ok(new { token });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN ERROR] {ex.GetType()}: {ex.Message}");
                return StatusCode(500, new
                {
                    error = ex.GetType().Name,
                    message = ex.Message
                });
            }
        }

        private string GenerateJwtToken(User user, string jwtSecret)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userId = user.Id ?? throw new InvalidOperationException("User ID is null");
            var email = user.Email ?? throw new InvalidOperationException("User Email is null");
            var username = user.Username ?? throw new InvalidOperationException("User Username is null");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim("username", username)
    };

            var issuer = _configuration["Jwt:Issuer"] ?? throw new Exception("Jwt:Issuer is missing");
            var audience = _configuration["Jwt:Audience"] ?? throw new Exception("Jwt:Audience is missing");
            var expiryStr = _configuration["Jwt:ExpiryMinutes"] ?? throw new Exception("Jwt:ExpiryMinutes is missing");

            if (!double.TryParse(expiryStr, out var expiryMinutes))
                throw new Exception("Jwt:ExpiryMinutes must be a valid number");

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
