using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.ApplicationModel.UserDataTasks.DataProvider;
using Windows.Storage.Streams;

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

        // Method for scan button
        private void FindInSpotify(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("FindInSpotify called");
            ReadThroughFiles();

            Debug.WriteLine("Finished Reading through files and searching in Spotify");
            // Clear the previous search results from the UI
            MainResultsPageUI.Items.Clear();

            // Iterate through the search results and display them in the UI
            foreach (var searchResponse in Data.SearchList)
            {
                Debug.WriteLine("Attempting to add UI for search result");
                string title = searchResponse.tracks.items[0].name;
                string artist = searchResponse.tracks.items[0].artists[0].name;
                string album = searchResponse.tracks.items[0].album.name;
            }
        }

        // Get the music file paths for the folder
        private async void ReadThroughFiles()
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
                    AddToMusicList(await ParseMusicFile(MusicFilePath));
                }
            }

            SearchAndDisplay();
        }

        private async void SearchAndDisplay()
        {
            try
            {
                Search search = new Search();

                Debug.WriteLine("Iterating through MusicList and searching Spotify for each song...");

                int searchIndex = 0;
                foreach (MusicInfo musicInfo in MusicList)
                {
                    if (Data.SpotifyToken != null)
                    {
                        // Search using spotify api and return its search results
                        var searchResults = await search.SearchSong(Data.SpotifyToken, musicInfo);

                        // Assign the search index to the search results for tracking later
                        searchResults.searchIndex = searchIndex;
                        searchIndex++;  // Increment search index for next iteration

                        // Add the result ids for each search to a list of strings that get added to broader search list
                        string result1 = searchResults.tracks.items[0].id;
                        string result2 = searchResults.tracks.items[1].id;
                        string result3 = searchResults.tracks.items[2].id;

                        Data.SearchSelection.Add(new List<string>() { result1, result2, result3 });

                        if (searchResults != null)
                        {
                            Debug.WriteLine($"Search results for {musicInfo.Title}: {searchResults.tracks.items.Count} items found.");
                            for (int i = 0; i < 3; i++)
                            {
                                Debug.WriteLine($"Item {i + 1}: {searchResults.tracks.items[i].name} by {string.Join(", ", searchResults.tracks.items[i].artists.Select(a => a.name))}");
                            }

                            DisplayUI(musicInfo, searchResults);

                            Debug.WriteLine("Adding search results to Data.SearchList...");
                            Data.SearchList.Add(searchResults);
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"SpotifyToken missing. spotifyToken: {Data.SpotifyToken}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during search: {ex.Message}");
            }
        }

        // Method to display the search results in the UI
        private void DisplayUI(MusicInfo musicFile, SpotifySearchResponse searchResults)
        {
            try
            {
                OriginalFileAndResult originalFileAndResult = new OriginalFileAndResult(musicFile, searchResults);
                // Add an item to the GridViews using the item Template in XAML
                MainResultsPageUI.Items.Add(originalFileAndResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error displaying search results for {musicFile.Title} by {musicFile.Artist}: {ex.Message}");
            }
        }

        // Parse the metadata for the music file
        private async Task<MusicInfo> ParseMusicFile(string filePath)
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

                // Get byte array of the music cover from the file metadata
                var image = file.Tag.Pictures.First().Data;
                BitmapImage bitmapImage = new BitmapImage();    // Bitmap Image to store the data later

                // 
                using (var stream = new InMemoryRandomAccessStream())
                {
                    using(var writer = new DataWriter(stream))
                    {
                        writer.WriteBytes(image.Data);
                        writer.StoreAsync().Wait();
                        writer.FlushAsync().Wait();
                        writer.DetachStream();
                    }
                    stream.Seek(0);
                    bitmapImage.SetSource(stream);
                }

                // Convert to ImageSource
                ImageSource imageSource = bitmapImage;

                // Create musicfile object
                return new MusicInfo(file.Tag.Title, file.Tag.FirstAlbumArtist, file.Tag.Album, filePath, imageSource);
            }

            // Catch wrong format exceptions
            catch (UnsupportedFormatException e)
            {
                Debug.WriteLine("UnsupportedFormatException: " + e.Message);
                return new MusicInfo("", "", "", filePath, null);
            }
        }

        // Add local music metadata to the observable collection
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

        private void SelectItemFromSearch(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Data.SearchSelection[(e.AddedItems.First() as SpotifySearchResponse).searchIndex] = ;
                Debug.WriteLine($"Selected ID: ");
            }
            catch
            {
                Debug.WriteLine($"Error upon selecting song");
            }
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
