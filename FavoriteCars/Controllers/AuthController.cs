using FavoriteCars.Models;
using FavoriteCars.RequestResponseObjects;
using FavoriteCars.Utililities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace FavoriteCars.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly JwtSettings _jwtSettings;
        

        public AuthController(DatabaseContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }


        [HttpPost("Login")]
        public async Task<ActionResult> Login(AuthLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return BadRequest("Invalid credentials");
            }

            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if(!passwordCorrect)
            {
                return BadRequest("Invalid credentials");
            }

            var token = _jwtSettings.GenerateJwt(request.Username);

            return Ok(new { Token = token });
        }


        [HttpPost("Register")]
        public async Task<ActionResult> Register(AuthRegisterRequest request)
        {
            if(UsernameExists(request.Username)) {
                return BadRequest("Username taken");
            }

            if(request.Username.Length < 4 || request.Password.Length < 4)
            {
                return BadRequest("Too short");
            }

            if(request.Password != request.ConfirmPassword)
            {
                return BadRequest("Passwords dont match");
            }


            var user = new User()
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool UsernameExists(string username)
        {
            return (_context.Users?.Any(u => u.Username == username)).GetValueOrDefault();
        }
    }
}
