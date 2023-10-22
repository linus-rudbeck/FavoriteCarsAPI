using FavoriteCars.RequestResponseObjects;
using FavoriteCars.Utililities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace FavoriteCars.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;

        public AuthController(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }


        [HttpPost("Login")]
        public async Task<ActionResult> Login(AuthLoginRequest request)
        {
            var user = await UsersService.VerifyUserCredentials(request.Username, request.Password);

            if (user == null)
            {
                return BadRequest("Invalid user credentials");
            }

            var token = _jwtSettings.GenerateJwt(request.Username);

            return Ok(new { Token = token });
        }


        [HttpPost("Register")]
        public async Task<ActionResult> Register(AuthRegisterRequest request)
        {
            var usernameExists = await UsersService.UsernameExists(request.Username);

            if (usernameExists) {
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
                Username = request.Username
            };

            var success = await UsersService.InsertUser(user, request.Password);

            if (success)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
