using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace LocalToSpotify
{
    internal class Search
    {
        private HttpClient client = new HttpClient();

        // Return json response from searching a song through Spotify API
        internal async Task<SpotifySearchResponse> SearchSong(string spotifyToken, MusicInfo info)
        {
            try
            {
                // Search query through spotify API
                StringBuilder sb = new StringBuilder("https://api.spotify.com/v1/search?q=");
                sb.Append(info.Title);
                sb.Append("+");
                sb.Append(info.Artist);
                sb.Append("+");
                sb.Append(info.Album);
                sb.Append("&type=track&limit=3");

                // Set the authorization header with your access token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken);

                // Make the GET request to the Spotify API
                var response = await client.GetAsync(sb.ToString());
                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response into a json object
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonObject>();
                    SpotifySearchResponse spotifySearch = JsonConvert.DeserializeObject<SpotifySearchResponse>(response.ToString());

                    // Debug output of search results
                    foreach (var item in spotifySearch.tracks.items)
                    {
                        Debug.WriteLine(item.name + " by " + item.artists);
                    }

                    return spotifySearch;
                }
                else
                {
                    throw new Exception($"Error fetching data: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return null; // or handle the error as needed
            }
        }
    }
}
