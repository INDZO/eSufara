using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace eSufara.Services
{
    class HashingService
    {
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Pretvori lozinku u bajtove
                var bytes = Encoding.UTF8.GetBytes(password);

                // Hashuj lozinku
                var hashedBytes = sha256.ComputeHash(bytes);

                // Pretvori byte[] u heksadecimalni string
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
