using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UsersService
    {
        public static async Task<User> GetUserByUsername(string username)
        {
            var db = new DatabaseConnection();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            return user;
        }

        public static async Task<bool> AddUser(User user)
        {
            var db = new DatabaseConnection();
            
            db.Users.Add(user);

            var result = await db.SaveChangesAsync();

            return result > 0;
        }

        public static async Task<List<User>> GetAllUsers()
        {
            var db = new DatabaseConnection();

            var users = await db.Users.ToListAsync();

            return users;
        }

        public static async Task<User> VerifyUserCredentials(string username, string password)
        {
            var user = await GetUserByUsername(username);

            if (user == null)
            {
                return null;    
            }

            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!passwordCorrect)
            {
                return null;
            }

            return user;
        }

        public static async Task<bool> UsernameExists(string username)
        {
            var db = new DatabaseConnection();
            var usernameExists = await db.Users.AnyAsync(u => u.Username == username);
            return usernameExists;
        }

        public static async Task<bool> InsertUser(User user, string password)
        {
            var db = new DatabaseConnection();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            db.Users.Add(user);

            var result = await db.SaveChangesAsync();

            return result > 0;
        }
    }
}
