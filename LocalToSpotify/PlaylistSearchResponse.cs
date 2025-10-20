using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    class PlaylistSearchResponse
    {
        [JsonProperty("href")]
        public string href;

        [JsonProperty("limit")]
        public int limit;

        [JsonProperty("next")]
        public string next;

        [JsonProperty("offset")]
        public int offset;

        [JsonProperty("previous")]
        public string previous;

        [JsonProperty("total")]
        public int total;

        [JsonProperty("items")]
        public List<PlaylistItem> playlistItems;
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class PlaylistExternalUrls
    {
        [JsonProperty("spotify")]
        public string spotify;
    }

    public class PlaylistImage
    {
        [JsonProperty("url")]
        public string url;

        [JsonProperty("height")]
        public int height;

        [JsonProperty("width")]
        public int width;
    }

    /*  Reference. Use class from Playlist.cs instead.
    public class Playlist
    {
        [JsonProperty("collaborative")]
        public bool collaborative;

        [JsonProperty("description")]
        public string description;

        [JsonProperty("external_urls")]
        public ExternalUrls external_urls;

        [JsonProperty("href")]
        public string href;

        [JsonProperty("id")]
        public string id;

        [JsonProperty("images")]
        public List<PlaylistImage> images;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("owner")]
        public Owner owner;

        [JsonProperty("public")]
        public bool @public;

        [JsonProperty("snapshot_id")]
        public string snapshot_id;

        [JsonProperty("tracks")]
        public PlaylistTracks tracks;

        [JsonProperty("type")]
        public string type;

        [JsonProperty("uri")]
        public string uri;
    }
    */

    public class Owner
    {
        [JsonProperty("external_urls")]
        public ExternalUrls external_urls;

        [JsonProperty("href")]
        public string href;

        [JsonProperty("id")]
        public string id;

        [JsonProperty("type")]
        public string type;

        [JsonProperty("uri")]
        public string uri;

        [JsonProperty("display_name")]
        public string display_name;
    }

    public class TrackAmount
    {
        [JsonProperty("href")]
        public string href;

        [JsonProperty("total")]
        public int total;
    }


}
