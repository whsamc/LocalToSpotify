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
        Data Data => Data.Instance;

        // Return json response from searching a song through Spotify API
        internal async Task<SpotifySearchResponse> SearchSong(string spotifyToken, MusicInfo info)
        {
            try
            {
                Debug.WriteLine("Building search query...");
                // Search query through spotify API
                StringBuilder sb = new StringBuilder("https://api.spotify.com/v1/search?q=");
                sb.Append(info.QueryTitle);
                sb.Append("%2B");
                sb.Append(info.QueryArtist);
                sb.Append("%2B");
                sb.Append(info.QueryAlbum);
                sb.Append("&type=track&limit=3");

                Debug.WriteLine("Adjusting search query parameters...");
                // Set the authorization header with your access token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken);

                Debug.WriteLine("Sending search request to Spotify API...");
                try
                {
                    // Make the GET request to the Spotify API
                    var response = await client.GetAsync(sb.ToString());

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Search request successful. Parsing JSON response into object...");
                        // Parse the JSON response into a json object
                        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonObject>();

                        Debug.WriteLine("Deserializing JSON response into SpotifySearchResponse object...");
                        SpotifySearchResponse spotifySearch = JsonConvert.DeserializeObject<SpotifySearchResponse>(jsonResponse.ToString());

                        Debug.WriteLine($"{info.Title} by {info.Artist} Results:");
                        // Debug output of search results
                        foreach (var item in spotifySearch.tracks.items)
                        {
                            Debug.WriteLine(item.name + " by " + item.artists.First().name);
                        }

                        return spotifySearch;
                    }
                    else
                    {
                        throw new Exception($"Error fetching data: {response.ReasonPhrase}");
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception during GET request: " + ex.Message);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return null;
            }
        }

        // Retrieve current user's playlists
        internal async Task<PlaylistResponse> SearchCurrentUserPlaylist(string spotifyToken)
        {
            try
            {
                Debug.WriteLine("Building playlist search query...");
                string url = "https://api.spotify.com/v1/me/playlists?limit=50";

                Debug.WriteLine("Adjusting search query parameters...");
                // Set the authorization header with your access token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken);

                try
                {
                    // Make the GET request to the Spotify API
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Search request successful. Parsing JSON response into object...");
                        // Parse the JSON response into a json object
                        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonObject>();

                        Debug.WriteLine("Deserializing JSON response into PlaylistResponse object...");
                        PlaylistResponse playlistSearch = JsonConvert.DeserializeObject<PlaylistResponse>(jsonResponse.ToString());

                        return playlistSearch;
                    }
                    else
                    {
                        Debug.WriteLine($"Error retrieving playlist: {response.StatusCode}");
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Exception occurred: {ex.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred: {ex.Message}");
                return null;
            }
        }

        internal void CreatePlaylist(string playlistName)
        {
            Debug.WriteLine("Creating url for api POST request...");
            string url = $"https://api.spotify.com/v1/users/{Data.UserProfile.id}/playlists";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Data.SpotifyToken);
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            // client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            var playlistData = new
            {
                name = playlistName,
                description = "",
                @public = false
            };
            // Serialize the playlist data to JSON
            var content = new StringContent(JsonConvert.SerializeObject(playlistData), Encoding.UTF8, "application/json");
            var response = client.PostAsync(url, content).Result;
            Debug.WriteLine($"Playlist POST request result: {response.StatusCode}");
        }
    }
}
