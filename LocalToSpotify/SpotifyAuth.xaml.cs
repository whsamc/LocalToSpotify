using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LocalToSpotify;

public partial class SpotifyAuth : ContentPage
{
	//
	string client_id = "CLIENT_ID";
	string redirect_uri = "LocalToSpotify://callback";
	string scope = "user-read-private playlist-read-private playlist-modify-private playlist-modify-public user-library-modify user-library-read ugc-image-upload";
	Uri authURI = new Uri("https://accounts.spotify.com/authorize");

	// 
    string spotifyCode;
	string spotifyCodeChallenge;

    // Method to open Spotify authentication Page
    public SpotifyAuth()
	{
        // Loads the page with XAML stuff
        InitializeComponent();
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
		for(int i = 0; i < sec.Length; i++)
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
	async void AuthorizeSpotifyPage(object sender, EventArgs e)
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
            {"redirect_uri", "LocalToSpotify://callback"}
        };

        // combine the url with the parameters
        var authUrl = QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize", parameters);

        // create a new uri
        Uri uri = new Uri(authUrl);
        await Launcher.Default.OpenAsync(uri);

        var authResult = await WebAuthenticator.AuthenticateAsync(
            new Uri(authUrl),
            new Uri("LocalToSpotify://callback"));

        // This shit dont work, but maybe it will later
        // https://github.com/microsoft/WindowsAppSDK/issues/441
        /*
        try
        {
            WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri("LocalToSpotify://callback"));
        }
        catch(TaskCanceledException eh)
        {
            // whoops
        }
        */
    }

    // Goes back to the main page
    async void BackToPage (object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

    // Changes the client id string whenever the entrytext is changed
    private void spotifyClientIDEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        client_id = e.NewTextValue;
    }

    // Open spotify auth page
    async private void LoginToSpotifyButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("Clicked on Spotify Login");
        await Navigation.PushAsync(new SpotifyAuth(), true);
    }


    // Needed to get responses from authenticating Spotify Login via browser
    public class RestService
    {
        HttpClient _client;
        JsonSerializerOptions _serializerOptions;

        public RestService()
        {
            _client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
    }
}