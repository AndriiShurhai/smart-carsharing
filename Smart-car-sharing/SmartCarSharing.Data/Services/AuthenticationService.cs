using Microsoft.EntityFrameworkCore;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data;
using System;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SmartCarSharing.Data.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;

        public AuthenticationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RegisterUserAsync(string name, string email, string password)
        {
            var emailExists = await _context.users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());

            if (emailExists)
            {
                throw new InvalidOperationException("Користувач з такою електронною поштою вже існує.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Name = name,
                Email = email,
                HashedPassword = hashedPassword
            };

            _context.users.Add(newUser);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                return null;
            }

            return user;
        }
    }
}