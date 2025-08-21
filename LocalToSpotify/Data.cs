using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    internal class Data
    {
        private string _spotifyToken = "";
        public string SpotifyToken { get { return _spotifyToken; } set { _spotifyToken = value; } }
        public Profile userProfile = new Profile();
    }
}
