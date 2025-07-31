using System;
using System.Collections.Generic;
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
        private string configFilePath = @"LocalToSpotify\Config\Config.dat";

        internal void EncryptStringToFile(string plainText)
        {
            try
            {
                FileStream fStream = new FileStream(configFilePath, FileMode.OpenOrCreate);   // This method covers if the file exists or 

                // Encrypt string to task to wait for completion
                IBuffer encryptedText = (IBuffer)ProtectAsync(plainText);

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

            // Return encrypted text as buffer
            return buffProtected;
        }

        private async void WriteEncryptionToFile(FileStream stream, IBuffer encrypted)
        {
            await stream.WriteAsync(encrypted.ToArray());

            stream.Close();
        }
    }
}
