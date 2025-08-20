using Microsoft.Security.Authentication.OAuth;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TagLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using WinRT.Interop;


namespace LocalToSpotify
{
    public sealed partial class SpotifyAuth : Page
    {
        private string client_id = "72eb2cc5a8bc438b9488a36f425f2dfc"; // Client ID is replaced by the user
        string client_secret = "CLIENT_SECRET"; // Client Secret is replaced by the user
        string redirect_uri = "LocalToSpotify://callback";  // The app's redirect URI, which is used to redirect the user back to the app after authentication
        string scope = "user-read-private playlist-read-private playlist-modify-private playlist-modify-public user-library-modify user-library-read ugc-image-upload";
        string authUriString = "https://accounts.spotify.com/authorize";
        string tokenUriString = "https://accounts.spotify.com/api/token";
        private string spotifyCode;
        private string spotifyCodeChallenge;
        private Encrypt encrypt = new Encrypt();

        private HttpClient client = new HttpClient();

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
        private async void AuthorizeSpotifyPage(object sender, RoutedEventArgs e)
        {
            // secret stuff to make it safe
            spotifyCode = CodeVerifier();
            spotifyCodeChallenge = CodeChallenge(spotifyCode);

            // Creates a dictionary with all the parameters needed to authenticate and login
            // UNUSED
            /*
            var parameters = new Dictionary<string, string>
            {
                {"response_type", "code"},
                {"client_id", client_id},
                {"scope", scope },
                {"code_challenge_method", "S256" },
                {"code_challenge", spotifyCodeChallenge},
                {"redirect_uri", redirect_uri}
            };
            */
            
            // Setting up to get authorization code by combining url with parameters
            AuthRequestParams authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest(client_id, new Uri(redirect_uri));

            authRequestParams.ResponseType = "code";
            authRequestParams.Scope = scope;
            authRequestParams.CodeChallengeMethod = CodeChallengeMethodKind.S256;
            authRequestParams.CodeChallenge = spotifyCodeChallenge;
            authRequestParams.State = Guid.NewGuid().ToString(); // Random state to prevent CSRF attacks

            AuthRequestResult authRequestResult = await OAuth2Manager.RequestAuthWithParamsAsync(MainWindow.MyAppWindow.OwnerWindowId, new Uri(authUriString), authRequestParams);

            Debug.WriteLine("TEST SPOTIFYAUTH WindowID: " + MainWindow.MyAppWindow.OwnerWindowId.ToString());
            Debug.WriteLine("AuthRequestResult: " + authRequestResult.ToString());

            AuthResponse authResponse = authRequestResult.Response;

            if (authResponse != null)
            {
                //To obtain the authorization code
                GetAuthorizationToken(authResponse);
            }
            else
            {
                AuthFailure authFailure = authRequestResult.Failure;
                // NotifyFailure(authFailure.Error, authFailure.ErrorDescription);
            }
        }

        // Exchange the authorization code for an access token
        private async void GetAuthorizationToken(AuthResponse response)
        {
            // Token request parameters
            TokenRequestParams tokenRequestParams = TokenRequestParams.CreateForAuthorizationCodeRequest(response);
            ClientAuthentication clientAuth = ClientAuthentication.CreateForBasicAuthorization(client_id, client_secret);

            // Dictionary to add additional parameters
            var additionalParams = new Dictionary<string, string>
            {
                {"method", "POST"},
                {"Content-Type",  "application/x-www-form-urlencoded"}
            };

            // extra parameters
            tokenRequestParams.GrantType = "authorization_code";
            tokenRequestParams.Code = response.Code;
            tokenRequestParams.RedirectUri = new Uri(redirect_uri);

            try
            {
                // Requesting the token using OAuth2Manager
                TokenRequestResult tokenRequestResult = await OAuth2Manager.RequestTokenAsync(new Uri(tokenUriString), tokenRequestParams, clientAuth);

                string spotifyToken = tokenRequestResult.Response.AccessToken; // Save spotify token to variable
                string refreshToken = tokenRequestResult.Response.RefreshToken; // Save refresh token to variable

                await encrypt.EncryptStringToFile(refreshToken);  // Encrypt the refresh token and save it to a file

                // Get authorization from spotify with token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken);
                var apiresponse = client.GetFromJsonAsync<Profile>("https://api.spotify.com/v1/me").Result; // GET the user profile from Spotify API

                Debug.WriteLine("SpotifyAuth userProfile: " + apiresponse.display_name);

                MainPage mainpage = App.mainPage; // Get the current MainPage instance

                // Bind the user profile to the MainPage variable userProfile
                mainpage.userProfile = apiresponse;
                
                Debug.WriteLine("Main Page userProfile: " + mainpage.userProfile.display_name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error requesting token: " + ex.Message);
                return;
            }

            Frame.Navigate(typeof(MainPage), App.mainPage);
        }

        internal async void CheckRefreshToken()
        {
            // Check if the config file exists, before trying to read from it
            if (System.IO.File.Exists(Encrypt.configFilePath))
            {
                // Decrypt the refresh token from the file
                string refreshToken = await encrypt.DecryptFromFile();

                try
                {
                    /* Refresh token parameters require the following:
                     *      grant_type= "refresh_token"
                     *      refresh_token = {refresh_token}
                     *      client_id = {client_id}
                     *      
                     * Also the following headers:
                     *      Content-Type = "application/x-www-form-urlencoded"
                     *      Authorization = "Basic {base64(client_id:client_secret)}"
                     */

                    // Set up the token request parameters for refreshing the token
                    TokenRequestParams tokenRequestParams = TokenRequestParams.CreateForRefreshToken(refreshToken);
                    tokenRequestParams.ClientId = client_id; // Set the client ID

                    // Dictionary to add additional parameters
                    var additionalParams = new Dictionary<string, string>
                    {
                        {"method", "POST" },
                        {"Content-Type",  "application/x-www-form-urlencoded"}
                    };

                    // extra parameters
                    tokenRequestParams.GrantType = "refresh_token";
                    TokenRequestResult tokenRequestResult = await OAuth2Manager.RequestTokenAsync(new Uri(tokenUriString), tokenRequestParams);


                    // Check if the token request was successful. If so, use the access token to get the user profile
                    if (tokenRequestResult.Response != null)
                    {
                        string spotifyToken = tokenRequestResult.Response.AccessToken;
                        string newRefreshToken = tokenRequestResult.Response.RefreshToken; // Get the new refresh token

                        await encrypt.EncryptStringToFile(newRefreshToken); // Encrypt the new refresh token and save it to a file

                        // Get authorization from spotify with token
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken);
                        var apiresponse = client.GetFromJsonAsync<Profile>("https://api.spotify.com/v1/me").Result; // GET the user profile from Spotify API

                        Debug.WriteLine("SpotifyAuth userProfile: " + apiresponse.display_name);

                        MainPage mainpage = App.mainPage; // Get the current MainPage instance

                        // Bind the user profile to the MainPage variable userProfile
                        mainpage.userProfile = apiresponse;

                        Debug.WriteLine("Main Page userProfile: " + mainpage.userProfile.display_name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error refreshing token: " + ex.Message);
                    return;
                }
            }
        }

        // Goes back to the main page
        private void BackToPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), App.mainPage);
        }

        // Changes the client id string whenever the entrytext is changed
        private void spotifyClientIDEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            client_id = spotifyClientIDEntry.Text;
        }

        // Changes the client secret string whenever the entrytext is changed
        private void spotifyClientSecretEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            client_secret = spotifyClientSecretEntry.Text;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("Navigated to...");

            // Check if the parameter is a Uri. If so, page was navigated to from browser
            if(e.Parameter is Uri)
            {
                Debug.WriteLine("Redirected to from browser");
            }
        }
    }
}
