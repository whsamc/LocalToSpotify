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
    /*  Separate encrypted files for client ID/Secret and token
     * 
     * 
     * 
     */
    internal class Encrypt
    {
        private string securityDescriptor = "LOCAL=user"; // Security descriptor for the DataProtectionProvider
        // private string configFilePath = @"\Config.dat";
        public static string configFilePath = @"E:\Source\Stuff\Config.dat";
        // public static string configFilePath = @"C:\Users\Sam\Documents\LocalToSpotify\Config.dat";


        internal async Task EncryptStringToFile(string plainText)
        {
            Debug.WriteLine("Attempting to encrypt text...");
            try
            {
                FileStream fStream = new FileStream(configFilePath, FileMode.OpenOrCreate);   // This method covers if the file exists or not

                Debug.WriteLine("Creating and opening file...");

                // Encrypt string to task to wait for completion
                var encryptedTextTask = ProtectString(plainText);

                // Await the task to get the encrypted text
                IBuffer encryptedText = await encryptedTextTask;

                WriteEncryptionToFile(fStream, encryptedText);
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
        }

        internal async Task<string> DecryptFromFile()
        {
            try
            {
                FileStream fstream = new FileStream(configFilePath, FileMode.Open); // Open the file

                Debug.WriteLine("Opening file for decryption...");

                // Read the encrypted text from the file and convert it to IBuffer after waiting for the task
                var decryptedTextTask = ReadBufferFromFile(fstream);
                IBuffer encryptedText = await decryptedTextTask;

                // Unprotect the encrypted text to get the original string after waiting for the task
                var unprotectedTextTask = UnprotectString(encryptedText);
                string unprotectedText = await unprotectedTextTask;

                fstream.Close();

                Debug.WriteLine("Decrypted text...");

                return unprotectedText; // Return the decrypted string
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                return string.Empty; // Return an empty string in case of error
            }
        }

        private async Task<IBuffer> ProtectString(string plainText)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            // Descriptor can be "LOCAL=user" for user-specific encryption or "LOCAL=machine" for machine-wide encryption.
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
            await stream.WriteAsync(encrypted.ToArray());   // Writing encrypted buffer to the file stream

            stream.Close();

            Debug.WriteLine("Written encrypted text to file...");
        }

        private async Task<IBuffer> ReadBufferFromFile(FileStream stream)
        {
            byte[] buffer = new byte[stream.Length];    // Byte array to hold data from file
            await stream.ReadAsync(buffer, 0, (int)stream.Length);  // Read the file into the byte array

            Debug.WriteLine("Reading buffer from file...");

            IBuffer protectedBuffer = CryptographicBuffer.CreateFromByteArray(buffer); // Convert byte array to IBuffer

            Debug.WriteLine("Converted byte array to IBuffer...");

            return protectedBuffer;
        }

        private async Task<string> UnprotectString(IBuffer protectedBuffer)
        {
            // Create a DataProtectionProvider object. Don't need descriptor here since we're unprotecting.
            DataProtectionProvider dpp = new DataProtectionProvider();

            // Decrypt message to IBuffer
            IBuffer unprotectedBuffer = await dpp.UnprotectAsync(protectedBuffer);

            // Convert the decrypted buffer back to a string and from binary to the actual information. Use same encoding as before.
            string unprotectedString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, unprotectedBuffer);

            return unprotectedString;
        }
    }
}
