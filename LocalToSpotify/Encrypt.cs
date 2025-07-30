using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace LocalToSpotify
{
    public class Encrypt
    {
        private string securityDescriptor = "LOCAL=user"; // Security descriptor for the DataProtectionProvider

        private void EncryptStringToFile(string plainText, string filePath)
        {
            try
            {
                FileStream fStream = new FileStream("Config.dat", FileMode.OpenOrCreate);   // This method covers if the file exists or not

                byte[] entropy = CreateRandomEntropy(); // Create random entropy

                // Encrypt string
                var x = ProtectAsync(plainText);
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
        }

        private static byte[] CreateRandomEntropy()
        {
            return RandomNumberGenerator.GetBytes(16); // Create random entropy and return it
        }


        private async Task<IBuffer> ProtectAsync(string plainText)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider dpp = new DataProtectionProvider(securityDescriptor);

            // Encode the plaintext input message to a buffer using the specified encoding
            IBuffer buffBinary = CryptographicBuffer.ConvertStringToBinary(plainText, BinaryStringEncoding.Utf8);

            // Encrypt the message.
            IBuffer buffProtected = await dpp.ProtectAsync(buffBinary);

            // Execution of the SampleProtectAsync function resumes here after the awaited task (Provider.ProtectAsync) completes.
            return buffProtected;
        }
    }
}
