using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2
{
    public static class AuthHelper
    {
        /// <summary>
        /// Uniqueness: It should be unique across all client applications registered with your authorization server.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateClientId(int length = 32)
        {

            //return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length)
            //.Replace("+", "").Replace("/", "").Replace("=", "");

            return Guid.NewGuid().ToString().Replace("-", "");
        }

        public static string GenerateClientSecret(int length = 64)
        {
            //return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length))
            //    .Replace("+", "").Replace("/", "").Replace("=", "");

            byte[] secretBytes = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }
            string clientSecret = Convert.ToBase64String(secretBytes).Replace("+", "").Replace("/", "").Replace("=", "");

            return clientSecret;
        }

        /// <summary>
        /// loading a certificate from a certificate store.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static X509Certificate2 LoadCertificateFromStore()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                try
                {
                    store.Open(OpenFlags.ReadOnly);

                    var certs = store.Certificates
                        .Find(X509FindType.FindByThumbprint, "252f105e20907222afcd3a744e5a75ea9fa3ae9b", true);

                    if (certs.Count > 0)
                        return certs[0];

                    // write to file
                    string logFilePath = "error_log.txt";
                    // Create the file if it does not exist
                    if (!File.Exists(logFilePath))
                    {
                        File.Create(logFilePath).Dispose(); // Create and close the file
                    }
                    File.AppendAllText(logFilePath, $"{DateTime.Now}: The certificate couldn't be found.\n"); // Log the exception message to a file

                    throw new InvalidOperationException("The certificate couldn't be found.");
                }
                catch (Exception ex)
                {
                    // write to file
                    string logFilePath = "error_log.txt";
                    // Create the file if it does not exist
                    if (!File.Exists(logFilePath))
                    {
                        File.Create(logFilePath).Dispose(); // Create and close the file
                    }
                    File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.Message}\n"); // Log the exception message to a file

                    throw new InvalidOperationException(ex.Message);
                }
            }
        }

        public static X509Certificate2 LoadSigningCertificateFromStore()
        {
            try
            {
                using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);

                    // Replace with your certificate's thumbprint
                    var certs = store.Certificates
                        .Find(X509FindType.FindByThumbprint, "252f105e20907222afcd3a744e5a75ea9fa3ae9b", true);

                    if (certs.Count > 0 && certs[0].HasPrivateKey)
                    {
                        return certs[0];
                    }

                    // write to file
                    string logFilePath = "error_log.txt";
                    // Create the file if it does not exist
                    if (!File.Exists(logFilePath))
                    {
                        File.Create(logFilePath).Dispose(); // Create and close the file
                    }
                    File.AppendAllText(logFilePath, $"{DateTime.Now}: The signing certificate couldn't be found or doesn't contain a private key.\n"); // Log the exception message to a file

                    throw new InvalidOperationException("The signing certificate couldn't be found or doesn't contain a private key.");
                }
            }
            catch (Exception ex)
            {
                string logFilePath = "error_log.txt";
                // Create the file if it does not exist
                if (!File.Exists(logFilePath))
                {
                    File.Create(logFilePath).Dispose(); // Create and close the file
                }
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.ToString()}.\n"); // Log the exception message to a file
            }

            throw new InvalidOperationException("The signing certificate couldn't be found or doesn't contain a private key.");

        }
    }
}
