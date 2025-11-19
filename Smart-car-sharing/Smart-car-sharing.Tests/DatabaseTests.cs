using Xunit;
using SmartCarSharingApp.Core;
using SmartCarSharing.Data;
using Microsoft.EntityFrameworkCore;
using SmartCarSharing.Core;

namespace SmartCarSharing.Tests
{
    public class DatabaseTests
    {
        private AppDbContext _context;

        public DatabaseTests()
        {
            // 1. Create a new set of options for an in-memory database.
            // We give it a unique database name so it's fresh for every test run.

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            //2. Create a new context instance
            _context = new AppDbContext(options);
        }

        [Fact]
        public async Task Add_ShouldSaveUserToDatabase()
        {
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                HashedPassword = "123"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");

            Assert.NotNull(savedUser);
            Assert.Equal("Test User", savedUser.Name);

        }
    }
}