using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.LocalToSpotifyVtableClasses;

namespace LocalToSpotify
{
    internal sealed class Data
    {
        // Singleton implementation for Data class.
        // Uses .NET 6+ Lazy<T> to ensure thread-safe, lazy initialization.

        private static readonly Lazy<Data> data = new Lazy<Data>(() => new Data());
        public static Data Instance { get { return data.Value; } }
        private Data() { }

        
        private string _spotifyToken = "";
        public string SpotifyToken { get { return _spotifyToken; } set { _spotifyToken = value; } }
        public Profile userProfile = new Profile();
        string? FileDirectory;
    }
}
