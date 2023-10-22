using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using FavoriteCars.Utililities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace FavoriteCars.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly IHubContext<RealTimeHub> _hubContext;
        private readonly JwtSettings _jwtSettings;

        public CarsController(IHubContext<RealTimeHub> hubContext, JwtSettings jwtSettings)
        {
            _hubContext = hubContext;
            _jwtSettings = jwtSettings;
        }

        // GET: api/Cars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
            var cars = await CarsService.GetAllCars();

            return Ok(cars);
        }

        // GET: api/Cars/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
            var car = await CarsService.GetCarById(id);

            if (car == null)
            {
                return NotFound();
            }

            return car;
        }

        // PUT: api/Cars/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCar(int id, Car car)
        {
            if (id != car.Id)
            {
                return BadRequest();
            }

            var success = await CarsService.UpdateCar(car);

            if (success)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }

        }

        // POST: api/Cars
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Car>> PostCar(Car car)
        {
            var success = await CarsService.InsertCar(car);

            if (success)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        // DELETE: api/Cars/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await CarsService.GetCarById(id);

            if (car == null)
            {
                return NotFound();
            }

            await CarsService.DeleteCar(car);

            return Ok();
        }

        [HttpPost("{id}/Like"), Authorize]
        public async Task<ActionResult> LikeCar(int id)
        {
            var car = await CarsService.GetCarById(id);

            if (car == null)
            {
                return NotFound();
            }

            var user = await GetUser();

            if (user == null)
            {
                return BadRequest();
            }

            // Skicka like-meddelande
            await _hubContext.Clients.All.SendAsync("CarLike", user.Username, car.Make, car.Model);

            return Ok();
        }

        private async Task<User> GetUser()
        {
            // Retrieve the Authorization header.
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            // Authorization: Bearer JWT_TOKEN
            var token = authHeader.Split(" ")[1];

            var username = _jwtSettings.GetUsername(token);

            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var user = await UsersService.GetUserByUsername(username);

            return user;
        }
    }
}
