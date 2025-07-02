using Microsoft.Security.Authentication.OAuth;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TagLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Formats.Asn1.AsnWriter;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LocalToSpotify
{
    public sealed partial class SpotifyAuth : Page
    {
        //
        string client_id = "CLIENT_ID";
        string redirect_uri = "LocalToSpotify://callback";
        string scope = "user-read-private playlist-read-private playlist-modify-private playlist-modify-public user-library-modify user-library-read ugc-image-upload";
        string authUriString = "https://accounts.spotify.com/authorize";
        string spotifyCode;
        string spotifyCodeChallenge;

        public SpotifyAuth()
        {
            this.InitializeComponent();
        }

        // Key
        private static string CodeVerifier()
        {
            // Random strings that can be generated for the key
            const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
            var random = new Random();

            // Generate a char array of defined length
            var sec = new char[128];

            // Generate random key using provided characters
            for (int i = 0; i < sec.Length; i++)
            {
                sec[i] = chars[random.Next(chars.Length)];
            }

            // return the generated key
            return new string(sec);
        }

        // Hash
        private static string CodeChallenge(string codeVerifier)
        {
            // Create an instance of SHA256 to be able to generate a hash
            using var sha256 = SHA256.Create();

            // Create a new hash using the generated key from CodeVerifier
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

            // Convert the hash into a string format
            var b64Hash = Convert.ToBase64String(hash);

            // Replace strings that match the pattern within the hash string with anothe string
            var code = Regex.Replace(b64Hash, "\\+", "-");
            code = Regex.Replace(code, "\\/", "_");
            code = Regex.Replace(code, "=+$", "");

            // Return the string
            return code;
        }

        // Open the authentication page through user's browser
        async void AuthorizeSpotifyPage(object sender, RoutedEventArgs e)
        {
            // secret stuff to make it safe
            spotifyCode = CodeVerifier();
            spotifyCodeChallenge = CodeChallenge(spotifyCode);

            // Creates a dictionary with all the parameters needed to authenticate and login
            var parameters = new Dictionary<string, string>
            {
                {"response_type", "code"},
                {"client_id", client_id},
                {"scope", scope },
                {"code_challenge_method", "S256" },
                {"code_challenge", spotifyCodeChallenge},
                {"redirect_uri", redirect_uri}
            };
            
            // Setting up to get authorization code by combining url with parameters
            AuthRequestParams authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest(client_id, new Uri(redirect_uri));

            authRequestParams.ResponseType = "code";
            authRequestParams.Scope = scope;
            authRequestParams.CodeChallengeMethod = CodeChallengeMethodKind.S256;
            authRequestParams.CodeChallenge = spotifyCodeChallenge;

            AuthRequestResult authRequestResult = await OAuth2Manager.RequestAuthWithParamsAsync(MainWindow.MyAppWindow.OwnerWindowId, new Uri(authUriString), authRequestParams);
        }


        // Goes back to the main page
        private void BackToPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainWindow));
        }

        // Changes the client id string whenever the entrytext is changed
        private void spotifyClientIDEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            client_id = spotifyClientIDEntry.Text;
        }
    }
}
