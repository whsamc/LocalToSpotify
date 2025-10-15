using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    public class Profile
    {
        public string country { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        public ExplicitContent explicit_content { get; set; }
        public ProfileExternalUrls external_urls { get; set; }
        public Followers followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<ProfileImage> images { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ExplicitContent
    {
        public bool filter_enabled { get; set; }
        public bool filter_locked { get; set; }
    }

    public class ProfileExternalUrls
    {
        public string spotify { get; set; }
    }

    public class Followers
    {
        public string href { get; set; }
        public int total { get; set; }
    }

    public class ProfileImage
    {
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}
