using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLib;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LocalToSpotify
{
    public sealed partial class MainPage : Page
    {
        string? FileDirectory;
        MainPage Current;
        public string DisplayName { get; set; }
        Data Data => Data.Instance;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            DataContext = Current;
        }

        // ObservableCollection needs to be a property to properly data bind
        public ObservableCollection<MusicInfo> MusicList { get; set; } = new ObservableCollection<MusicInfo>();

        private List<string> MusicFilePathList = new List<string>();

        private HashSet<string> fileExtensions = new HashSet<string>()
        {
            ".mp3",
            ".wav",
            ".flac",
            ".opus",
            ".m4a",
            ".wma",
            ".aac"
        };

        private void FindInSpotify(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ReadThroughFiles called");
            ReadThroughFiles(sender, e);
        }

        // Get the music file paths for the folder
        private async void ReadThroughFiles(object sender, RoutedEventArgs e)
        {
            // Return if FileDirectory is null
            if (FileDirectory == null) return;

            Debug.WriteLine("Clearing previous data...");
            MusicFilePathList.Clear();
            MusicList.Clear();

            Debug.WriteLine("Trimming filepath string...");
            // trim quotation marks
            FileDirectory = FileDirectory.Trim('"');

            // If the path is actually just a file
            if (System.IO.File.Exists(FileDirectory))
            {
                Debug.WriteLine("File path is a single file. Adding to list...");
                // Add single item to the list because of the parse method later
                MusicFilePathList.Add(FileDirectory);
            }
            // If the path is a directory
            else
            {
                Debug.WriteLine("File path is a directory. Searching for music files... Adding to file path list...");
                // get file paths for all music files inside folder
                MusicFilePathList = Directory.GetFiles(FileDirectory, "*", SearchOption.AllDirectories).ToList();
            }

            Debug.WriteLine("Iterating through music file paths list...");
            // Iterate through list and parse metadata from each filepath
            foreach (var MusicFilePath in MusicFilePathList)
            {
                Debug.WriteLine("Checking file type...");
                // Check the file type with fileExtensions hashset
                if (fileExtensions.Contains(Path.GetExtension(MusicFilePath)))
                {
                    Debug.WriteLine("File type supported. Parsing music file and adding to music list...");
                    AddToMusicList(ParseMusicFile(MusicFilePath));
                }
            }

            try
            {
                Search search = new Search();
                
                foreach(MusicInfo musicInfo in MusicList)
                {
                    // Search using spotify api and return its search results
                    var searchResults = await search.SearchSong(Data.SpotifyToken, musicInfo);
                    if (searchResults != null)
                    {
                        Debug.WriteLine($"Search results for {musicInfo.Title}: {searchResults.tracks.items.Count} items found.");
                        for(int i = 0; i < 3; i++)
                        {
                            Debug.WriteLine($"Item {i+1}: {searchResults.tracks.items[i].name} by {string.Join(", ", searchResults.tracks.items[i].artists.Select(a => a.name))}");
                        }

                        Data.SearchList.Add(searchResults);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during search: {ex.Message}");
            }
        }

        // Parse the metadata for the music file
        private MusicInfo ParseMusicFile(string filePath)
        {
            // trim the string of quotation marks
            var path = filePath.Trim('"');

            // Try to create a MusicFile object with metadata properties
            try
            {
                Debug.WriteLine("Creating TagLib file object...");
                // File to open
                var file = TagLib.File.Create(@path);

                Debug.WriteLine("Creating MusicInfo object containing metadata");
                // Create musicfile object
                return new MusicInfo(file.Tag.Title, file.Tag.FirstAlbumArtist, file.Tag.Album, filePath);
            }

            // Catch wrong format exceptions
            catch (UnsupportedFormatException e)
            {
                Debug.WriteLine("UnsupportedFormatException: " + e.Message);
                return new MusicInfo("", "", "", filePath);
            }
        }

        private void AddToMusicList(MusicInfo song)
        {
            Debug.WriteLine("Adding MusicInfo object to MusicList observable collection...");
            MusicList.Add(song);
        }

        // Switch pages to the Spotify Authentication page
        private void SpotifyAuthPageButton_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SpotifyAuth), App.spotifyAuth);
        }

        // Set the FileDirectory string whenever the entry text box is changed
        private void ReadFileDirectoryPath(object sender, TextChangedEventArgs e)
        {
            FileDirectory = musicFileInput.Text;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(Data.UserProfile.display_name != null)
            {
                Debug.WriteLine("MainPage UserProfile: " + Data.UserProfile.display_name);
                DisplayName = Data.UserProfile.display_name;
            }
            else
            {
                Debug.WriteLine("No data for UserProfile.display_name");
            }
            Debug.WriteLine("Navigated to MainPage");
        }
    }
}
