using eSufara.Data;
using eSufara.Model;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows.Media.Imaging;

namespace eSufara.ViewModel
{
    internal class MainViewModel
    {
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var context = new eSufaraDbContext();
            return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        }

        public BitmapImage? LoadImageFromBytes(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            BitmapImage bitmap = new BitmapImage();
            using (var ms = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
            }

            bitmap.Freeze();
            return bitmap;
        }

        public async Task<bool> UpdateUserProfileImageAsync(int userId, byte[] imageData)
        {
            try
            {
                using var context = new eSufaraDbContext();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return false;

                user.ProfilePicture = imageData;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
