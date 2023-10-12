using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FavoriteCars.Models;
using FavoriteCars.Utililities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FavoriteCars.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<RealTimeHub> _hubContext;
        private readonly JwtSettings _jwtSettings;

        public CarsController(DatabaseContext context, IHubContext<RealTimeHub> hubContext, JwtSettings jwtSettings)
        {
            _context = context;
            _hubContext = hubContext;
            _jwtSettings = jwtSettings;
        }

        // GET: api/Cars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
          if (_context.Cars == null)
          {
              return NotFound();
          }
            return await _context.Cars.ToListAsync();
        }

        // GET: api/Cars/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
          if (_context.Cars == null)
          {
              return NotFound();
          }
            var car = await _context.Cars.FindAsync(id);

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

            _context.Entry(car).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(id))
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

        // POST: api/Cars
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Car>> PostCar(Car car)
        {
          if (_context.Cars == null)
          {
              return Problem("Entity set 'DatabaseContext.Cars'  is null.");
          }
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCar", new { id = car.Id }, car);
        }

        // DELETE: api/Cars/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            if (_context.Cars == null)
            {
                return NotFound();
            }
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/Like"), Authorize]
        public async Task<ActionResult> LikeCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if(car == null)
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

            if(string.IsNullOrEmpty(username))
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            return user;
        }

        private bool CarExists(int id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
