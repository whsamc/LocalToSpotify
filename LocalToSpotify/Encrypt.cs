using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace LocalToSpotify
{
    internal class Encrypt
    {
        private string securityDescriptor = "LOCAL=user"; // Security descriptor for the DataProtectionProvider
        // private string configFilePath = @"\Config.dat";
        private string configFilePath = @"E:\Source\Stuff\Config.dat";

        internal async Task EncryptStringToFile(string plainText)
        {
            try
            {
                Debug.WriteLine("Attempting to encrypt text...");

                FileStream fStream = new FileStream(configFilePath, FileMode.OpenOrCreate);   // This method covers if the file exists or not

                Debug.WriteLine("Creating and opening file...");

                // Encrypt string to task to wait for completion
                var encryptedTextTask = ProtectAsync(plainText);

                IBuffer encryptedText = await encryptedTextTask;

                WriteEncryptionToFile(fStream, encryptedText);
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
        }

        private async Task<IBuffer> ProtectAsync(string plainText)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider dpp = new DataProtectionProvider(securityDescriptor);

            // Encode the plaintext input message to a buffer using the specified encoding
            IBuffer buffBinary = CryptographicBuffer.ConvertStringToBinary(plainText, BinaryStringEncoding.Utf8);

            // Encrypt the message.
            IBuffer buffProtected = await dpp.ProtectAsync(buffBinary);

            Debug.WriteLine("Encrypted text...");

            // Return encrypted text as buffer
            return buffProtected;
        }

        private async void WriteEncryptionToFile(FileStream stream, IBuffer encrypted)
        {
            await stream.WriteAsync(encrypted.ToArray());

            stream.Close();

            Debug.WriteLine("Written encrypted text to file...");
        }
    }
}
