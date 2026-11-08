using eSufara.Model;
using Microsoft.EntityFrameworkCore;

namespace eSufara.Services
{
    internal class AuthService
    {
        private readonly Data.eSufaraDbContext _context;
        private readonly HashingService _hashingService;

        public AuthService(Data.eSufaraDbContext context, HashingService hashingService)
        {
            _hashingService = hashingService;
            _context = context;
        }

        public async Task<int> Register(string username, string password, byte[]? profilePicture)
        {
            username = NormalizeUsername(username);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return 0;

            var usernameKey = username.ToLower();
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == usernameKey);
            if (existingUser != null)
                return 0;

            var user = new User
            {
                Username = username,
                Password = _hashingService.HashPassword(password),
                ProfilePicture = profilePicture
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<int> LogIn(string username, string password)
        {
            username = NormalizeUsername(username);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return 0;

            var usernameKey = username.ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == usernameKey);

            if (user == null)
                return 0;

            var hashedEnteredPassword = _hashingService.HashPassword(password);
            if (user.Password != hashedEnteredPassword)
                return -1;

            return user.Id;
        }

        private static string NormalizeUsername(string username)
        {
            return username.Trim();
        }
    }
}
