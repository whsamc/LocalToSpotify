using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

    // Method to open Spotify authentication
    public async void SpotifyAuth_Clicked(Object sender, EventArgs e)
    {
        InitializeComponent();
        spotifyCode = CodeVerifier();
        spotifyCodeChallenge = CodeChallenge(spotifyCode);

        var parameters = new Dictionary<string, string>
        {
            {"response_type", "code"},
            {"client_id", client_id},
            {"scope", scope },
            {"code_challenge_method", "S256" },
            {"code_challenge", spotifyCodeChallenge},
            {"redirect_uri", "LocalToSpotify://callback"}
        };


        var authUrl = QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize", parameters);
        Uri uri = new Uri("https://accounts.spotify.com/authorize");
        await Launcher.Default.OpenAsync(uri);
    }

    public SpotifyAuth()
	{

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

	void AuthorizeSpotifyPage()
	{
		

        var parameters = new Dictionary<string, string>
            {                
				{"response_type", "code"},
                {"client_id", client_id},
				{"scope", scope },
				{"code_challenge_method", "S256" },
                {"code_challenge", spotifyCodeChallenge},
                {"redirect_uri", "LocalToSpotify://callback"}
            };
    }

    async void BackToPage (object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

}