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
        private List<List<string>> _searchSelection = new List<List<string>>();
        private ObservableCollection<SpotifySearchResponse> _searchList = new ObservableCollection<SpotifySearchResponse>();

        private Profile _userProfile = new Profile();
        private SpotifySearchResponse _searchResponse = new SpotifySearchResponse();

        public string SpotifyToken
        {
            get { return _spotifyToken; }
            set
            {
                _spotifyToken = value;
                OnPropertyChanged();
            }
        }

        public Profile UserProfile
        {
            get { return _userProfile; }
            set
            {
                _userProfile = value;
                OnPropertyChanged();
            }
        }

        public string FileDirectory
        {
            get { return _fileDirectory; }
            set
            {
                _fileDirectory = value;
                OnPropertyChanged();
            }
        }

        public SpotifySearchResponse SearchResponse
        {
            get { return _searchResponse; }
            set
            {
                _searchResponse = value;
                OnPropertyChanged();
            }
        }

        public List<List<string>> SearchSelection
        {
            get { return _searchSelection; }
            set
            {
                _searchSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SpotifySearchResponse> SearchList
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
