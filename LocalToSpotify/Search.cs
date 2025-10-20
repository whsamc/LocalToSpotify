using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;

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
        internal async Task<PlaylistSearchResponse> SearchCurrentUserPlaylist(string spotifyToken)
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
                        PlaylistSearchResponse playlistSearch = JsonConvert.DeserializeObject<PlaylistSearchResponse>(jsonResponse.ToString());

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

        internal bool CreatePlaylist()
        {
            if(Data.PlaylistName == null || Data.PlaylistName == "")
            {
                Debug.WriteLine("Playlist Name is empty");
                return false;
            }
            else
            {
                Debug.WriteLine("Creating url for api POST request...");
                string url = $"https://api.spotify.com/v1/users/{Data.UserProfile.id}/playlists";
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Data.SpotifyToken);
                var headers = client.DefaultRequestHeaders;

                if (!headers.UserAgent.TryParseAdd("Content-Type"))
                {
                    throw new Exception("Invalid header value: Content-Type");
                }

                var playlistData = new
                {
                    name = Data.PlaylistName,
                    description = "",
                    @public = false
                };
                // Serialize the playlist data to JSON
                var jsonString = JsonConvert.SerializeObject(playlistData);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                // POST request to create playlist
                var response = client.PostAsync(url, content);

                // Converting response into json object
                var jsonResponse = response.Result.Content.ReadFromJsonAsync<JsonObject>();
                Playlist newPlaylist = JsonConvert.DeserializeObject<Playlist>(jsonResponse.ToString());
                
                Debug.WriteLine($"Playlist POST request result: {response.Result.StatusCode}");
                Debug.WriteLine($"Playlist ID: {newPlaylist.id}");

                if(response.Result.IsSuccessStatusCode)
                {
                    AddItemsToPlaylist(newPlaylist.id);
                }
                return true;
            }
        }

        internal void AddItemsToPlaylist(string playlistID)
        {
            string url = $"https://api.spotify.com/v1/playlists/{playlistID}/tracks";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Data.SpotifyToken);
            var headers = client.DefaultRequestHeaders;

            if (!headers.UserAgent.TryParseAdd("Content-Type"))
            {
                throw new Exception("Invalid header value: Content-Type");
            }

            // Create array of track URI strings to add to playlist
            string[] stringArray = new string[Data.SearchSelection.Count];
            stringArray = Data.SearchSelection.ToArray();

            var playlistItems = new
            {
                uris = stringArray
            };

            // Serialize the playlist data to JSON
            var jsonString = JsonConvert.SerializeObject(playlistItems);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            // POST request to add items to playlist
            var response = client.PostAsync(url, content);

            if (response.Result.IsSuccessStatusCode)
            {
                Debug.WriteLine("Successfully added items to playlist");
            }
            else
            {
                Debug.WriteLine("Failed adding items to playlist");
            }
        }
    }
}
