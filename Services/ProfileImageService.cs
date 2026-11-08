using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace eSufara.Services
{
    public static class ProfileImageService
    {
        private const int MaxProfileImageBytes = 2 * 1024 * 1024;

        public static bool TryLoadProfileImage(string fileName, out BitmapImage bitmap, out byte[] imageBytes, out string errorMessage)
        {
            bitmap = new BitmapImage();
            imageBytes = [];
            errorMessage = "";

            try
            {
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Length > MaxProfileImageBytes)
                {
                    errorMessage = "Slika je prevelika. Izaberite sliku manju od 2 MB.";
                    return false;
                }

                imageBytes = File.ReadAllBytes(fileName);

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 512;
                bitmap.EndInit();
                bitmap.Freeze();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                errorMessage = "Slika nije mogla da se učita. Izaberite drugu JPG ili PNG sliku.";
                return false;
            }
        }

        public static byte[] LoadDefaultProfileImageBytes()
        {
            var baseDir = AppContext.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, "Resources", "Images", "default.png"),
                Path.Combine(baseDir, "Content", "default.png"),
                Path.Combine(baseDir, "default.png")
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                    return File.ReadAllBytes(path);
            }

            return [];
        }
    }
}
