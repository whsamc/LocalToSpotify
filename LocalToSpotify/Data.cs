using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinRT.LocalToSpotifyVtableClasses;

namespace LocalToSpotify
{
    internal sealed class Data : INotifyPropertyChanged
    {
        // Singleton implementation for Data class.
        // Uses .NET 6+ Lazy<T> to ensure thread-safe, lazy initialization.
        private static readonly Lazy<Data> data = new Lazy<Data>(() => new Data());
        public static Data Instance { get { return data.Value; } }
        private Data() { }

        //
        public event PropertyChangedEventHandler PropertyChanged;


        private string _spotifyToken = "";
        private string _fileDirectory = "";

        public Profile userProfile = new Profile();
        public string SpotifyToken
        {
            get { return _spotifyToken; }
            set
            {
                _spotifyToken = value;
                OnPropertyChanged();
            }
        }
        public string? FileDirectory;

        // Call this method whenever property is updated
        //  CallerMemberName
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
