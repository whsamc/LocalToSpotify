using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Diagnostics;

namespace LocalToSpotify
{
    class Search
    {
        private HttpClient client = new HttpClient();

        internal async Task<MusicList> SearchSong(string spotifyToken, string title, string artist, string album)
        {
            try
            {
                StringBuilder sb = new StringBuilder("https://api.spotify.com/v1/search?q=");
                sb.Append(title);
                sb.Append("+");
                sb.Append(artist);
                sb.Append("+");
                sb.Append(album);
                sb.Append("&type=track&limit=3");

                // Set the authorization header with your access token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken);

                // Make the GET request to the Spotify API
                var response = await client.GetAsync(sb.ToString());
                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response into a MusicFile object
                    MusicList musicList = await response.Content.ReadFromJsonAsync<MusicList>();
                    foreach(var song in musicList.Songs)
                    {
                        Debug.WriteLine(song.Title + " by " + song.Artist);
                    }

                    return musicList;
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
