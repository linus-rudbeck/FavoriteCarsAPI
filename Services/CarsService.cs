using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services
{
    public class CarsService
    {
        public static async Task<List<Car>> GetAllCars()
        {
            var db = new DatabaseConnection();

            var cars = await db.Cars.ToListAsync();

            return cars;
        }

        public static async Task<Car> GetCarById(int id)
        {
            var db = new DatabaseConnection();

            var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == id);

            return car;
        }

        public static async Task<bool> UpdateCar(Car car)
        {
            var db = new DatabaseConnection();

            db.Entry(car).State = EntityState.Modified;

            var result = await db.SaveChangesAsync();

            return result > 0;
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
