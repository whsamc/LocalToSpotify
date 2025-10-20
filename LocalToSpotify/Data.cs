using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private string _playlistName = "";
        private List<string> _searchSelection = new List<string>();
        private ObservableCollection<SpotifySearchResponse> _searchList = new ObservableCollection<SpotifySearchResponse>();

        private Profile _userProfile = new Profile();
        private SpotifySearchResponse _searchResponse = new SpotifySearchResponse();
        private PlaylistSearchResponse _playlistResponse = new PlaylistSearchResponse();

        internal string SpotifyToken
        {
            get { return _spotifyToken; }
            set
            {
                _spotifyToken = value;
                OnPropertyChanged();
            }
        }

        internal Profile UserProfile
        {
            get { return _userProfile; }
            set
            {
                _userProfile = value;
                OnPropertyChanged();
            }
        }

        internal string FileDirectory
        {
            get { return _fileDirectory; }
            set
            {
                _fileDirectory = value;
                OnPropertyChanged();
            }
        }

        internal string PlaylistName
        {
            get { return _playlistName; }
            set
            {
                _playlistName = value;
                OnPropertyChanged();
            }
        }

        internal SpotifySearchResponse SearchResponse
        {
            get { return _searchResponse; }
            set
            {
                _searchResponse = value;
                OnPropertyChanged();
            }
        }

        internal PlaylistSearchResponse UserPlaylists
        {
            get { return _playlistResponse; }
            set
            {
                _playlistResponse = value;
                OnPropertyChanged();
            }
        }

        internal List<string> SearchSelection
        {
            get { return _searchSelection; }
            set
            {
                _searchSelection = value;
                OnPropertyChanged();
            }
        }

        internal ObservableCollection<SpotifySearchResponse> SearchList
        {
            get { return _searchList; }
            set
            {
                _searchList = value;
                OnPropertyChanged();
            }
        }

        // Call this method whenever property is updated
        //  CallerMemberName
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
