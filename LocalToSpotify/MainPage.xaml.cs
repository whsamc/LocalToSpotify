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
        public static Profile userProfile = new Profile();
        MainPage Current;
        public string DisplayName { get; set; }
        
        private string _spotifyToken;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            DataContext = Current;
        }

        // ObservableCollection needs to be a property to properly data bind
        public ObservableCollection<MusicFile> MusicList { get; set; } = new ObservableCollection<MusicFile>();

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

        // Get the music file paths for the folder
        private void ReadThroughFiles(object sender, RoutedEventArgs e)
        {
            // Return if FileDirectory is null
            if (FileDirectory == null) return;

            MusicFilePathList.Clear();
            MusicList.Clear();

            // trim quotation marks
            FileDirectory = FileDirectory.Trim('"');

            // If the path is actually just a file
            if (System.IO.File.Exists(FileDirectory))
            {
                // Add single item to the list because of the parse method later
                MusicFilePathList.Add(FileDirectory);
            }
            // If the path is a directory
            else
            {
                // get file paths for all music files inside folder
                MusicFilePathList = Directory.GetFiles(FileDirectory, "*", SearchOption.AllDirectories).ToList();
            }

            // Iterate through list and parse metadata from each filepath
            foreach (var MusicFilePath in MusicFilePathList)
            {
                // Check the file type with fileExtensions hashset
                if (fileExtensions.Contains(Path.GetExtension(MusicFilePath)))
                {
                    AddToMusicList(ParseMusicFile(MusicFilePath));
                }
            }

            try
            {
                Search search = new Search();

                // Search using spotify api and return its search results
                search.SearchSong();
            }
        }

        // Parse the metadata for the music file
        private MusicFile ParseMusicFile(string filePath)
        {
            // trim the string of quotation marks
            var path = filePath.Trim('"');

            // Try to create a MusicFile object with metadata properties
            try
            {
                // File to open
                var file = TagLib.File.Create(@path);

                // Create musicfile object
                return new MusicFile(file.Tag.Title, file.Tag.FirstAlbumArtist, file.Tag.Album, filePath);
            }

            // Catch wrong format exceptions
            catch (UnsupportedFormatException e)
            {
                return new MusicFile("", "", "", filePath);
            }
        }

        private void AddToMusicList(MusicFile song)
        {
            MusicList.Add(song);
        }

        // Switch pages to the Spotify Authentication page
        private void SpotifyAuthPageButton_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SpotifyAuth));
        }

        // Set the FileDirectory string whenever the entry text box is changed
        private void ReadFileDirectoryPath(object sender, TextChangedEventArgs e)
        {
            FileDirectory = musicFileInput.Text;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(userProfile.display_name != null)
            {
                Debug.WriteLine("This is userProfile: " + userProfile.display_name);
                DisplayName = userProfile.display_name;
            }
            else
            {
                Debug.WriteLine("No data for userProfile.display_name");
            }
            Debug.WriteLine("Navigated to MainPage");
        }
    }
}
