using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using Repositories;

namespace Services
{
    public class CarsService : BaseService
    {
        public static async Task<List<Car>> GetAllCars()
        {
            var db = new DatabaseConnection();

            var cars = await db.Cars.ToListAsync();

            return cars;
        }

        public static async Task<Car> GetCarById(int id)
        {
            // 1
            var cacheKey = $"car:{id}";

            // 2
            var cachedCar = Redis.StringGet(cacheKey);

            if (cachedCar.HasValue)
            {
                // 3
                return JsonConvert.DeserializeObject<Car>(cachedCar);
            }

            // 4
            await Task.Delay(2000); // Simulate slow loading to demonstrate caching

            // 5
            var db = new DatabaseConnection();
            var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == id);

            // 6
            if (car != null)
            {
                Redis.StringSet(cacheKey, JsonConvert.SerializeObject(car), TimeSpan.FromHours(1));
            }

            return car;
        }

        public static async Task<bool> UpdateCar(Car car)
        {
            // 1
            var db = new DatabaseConnection();
            db.Entry(car).State = EntityState.Modified;
            var result = await db.SaveChangesAsync();
            var success = result > 0;

            if(success)
            {

                // 2
                var cacheKey = $"car:{car.Id}";

                // 3
                Redis.StringSet(cacheKey, JsonConvert.SerializeObject(car), TimeSpan.FromHours(1));
            }

            return success;
        }

        public static async Task<bool> InsertCar(Car car)
        {
            var db = new DatabaseConnection();

            db.Cars.Add(car);

            var result = await db.SaveChangesAsync();

            return result > 0;
        }

        public static async Task<bool> DeleteCar(Car car)
        {
            var db = new DatabaseConnection();

            db.Cars.Remove(car);
            var result = await db.SaveChangesAsync();

            return result > 0;
        }


    }
}
