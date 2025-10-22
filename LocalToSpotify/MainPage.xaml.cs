using ABI.Microsoft.UI.Xaml;
using Microsoft.UI.Text;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI;
using Color = Windows.UI.Color;
using CornerRadius = Microsoft.UI.Xaml.CornerRadius;
using RoutedEventArgs = Microsoft.UI.Xaml.RoutedEventArgs;
using RoutedEventHandler = Microsoft.UI.Xaml.RoutedEventHandler;
using Thickness = Microsoft.UI.Xaml.Thickness;


namespace LocalToSpotify
{
    /*  
     * 
     * 
     * 
     * 
     */


    public sealed partial class MainPage : Page
    {
        string? FileDirectory;
        private string? playlistName;
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
            ScanBtn.IsEnabled = false;  // Disable scan button while scanning

            // Clear the previous search results from the UI
            MainResultsPageUI.Items.Clear();
            PlaylistStackPanel.Children.Clear();
            ReadThroughFiles();
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
            // Disable the playlist buttons while searching
            CreatePlaylistBtn.IsEnabled = false;
            UpdatePlaylistBtn.IsEnabled = false;
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
                        foreach(var song in searchResults.tracks.items)
                        {
                            song.searchResponseIndex = searchIndex;
                        }
                        searchResults.searchOrder = searchIndex + 1;
                        searchIndex++;  // Increment search index for next iteration

                        // Add the result uri for each search to a list of strings that get added to broader search list
                        string result1 = searchResults.tracks.items[0].uri;
                        string result2 = searchResults.tracks.items[1].uri;
                        string result3 = searchResults.tracks.items[2].uri;

                        // Adding the first result's uri to the SearchSelection list as default
                        AddToSearchSelection(result1);

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

            // After finishing display
            CreatePlaylistBtn.IsEnabled = true;
            UpdatePlaylistBtn.IsEnabled = true;
            ScanBtn.IsEnabled = true;
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

        // Add searched music metadata to List<string> SearchSelection. Contains the selection for each search result (default:first item)
        private void AddToSearchSelection(string uri)
        {
            Debug.WriteLine("Adding search result uri to SearchSelection list...");
            Data.SearchSelection.Add(uri);
        }

        // Change searched music selection when the user selects a different item from the search results
        private void ChangeSearchSelection(int index, string uri)
        {
            Data.SearchSelection[index] = uri;
            Debug.WriteLine($"Changed search selection at index {index} to uri: {uri}");
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
                // Get the selected item from the gridview and also the index of which search it is
                //  Selected item gets added to AddedItems List as Grid object.
                //  The DataContext of the grid is set in XAML. Retrieve it as Item
                var item = (((e.AddedItems.First() as Grid).DataContext) as Item);
                var index = item.searchResponseIndex;

                // Change the selection for that search index to the selected item's ID
                ChangeSearchSelection(index, item.uri);
                Debug.WriteLine($"Index {index} selection changed to: {item.name} by {item.artists.First().name}");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error upon selecting song: {ex.Message}");
            }
        }

        private void CreateNewPlaylistMethod(object sender, RoutedEventArgs e)
        {
            Search search = new Search();

            ShowCreatePlaylistFunctionUI();
            // search.CreatePlaylist();

        }

        private void ShowCreatePlaylistFunctionUI()
        {
            Search search = new Search();

            Border border = new Border();
            border.CornerRadius = new CornerRadius(5);
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(255,255,255,255));
            border.BorderThickness = new Thickness(1);
            border.Width = 250;
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Padding = new Thickness(10, 10, 10, 10);

            StackPanel playlistHousing = new StackPanel();

            // Create a textbox for the user to input a name. Adjust parameters
            Microsoft.UI.Xaml.Controls.TextBox playlistNameBox = new Microsoft.UI.Xaml.Controls.TextBox();
            playlistNameBox.PlaceholderText = "Name of Playlist";
            playlistNameBox.TextChanged += PlaylistNameBox_TextChanged;
            playlistNameBox.Width = 250;
            playlistNameBox.HorizontalAlignment = HorizontalAlignment.Left;

            // Create a button to send POST request to create playlist with items
            Button sendPOSTrequestBtn = new Button();
            sendPOSTrequestBtn.Content = "Create Playlist";
            sendPOSTrequestBtn.Click += CreatePlaylistButton_Click;
            sendPOSTrequestBtn.Width = 110;
            sendPOSTrequestBtn.FontSize = 12;
            sendPOSTrequestBtn.FontWeight = FontWeights.Bold;
            sendPOSTrequestBtn.Margin = new Thickness(0, 10, 0, 0);
            sendPOSTrequestBtn.HorizontalAlignment = HorizontalAlignment.Center;

            // Add elements to new stackpanel and border
            border.Child = playlistHousing;
            playlistHousing.Children.Add(playlistNameBox);
            playlistHousing.Children.Add(sendPOSTrequestBtn);

            // Clear any existing UI elements and then add border element housing the stackpanel and button
            PlaylistStackPanel.Children.Clear();
            PlaylistStackPanel.Children.Add(border);
        }

        private void PlaylistNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // change field playlistName to user's input in textbox.
            Data.PlaylistName = (sender as Microsoft.UI.Xaml.Controls.TextBox).Text;
        }
        internal async void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            InvalidPlaylistNameInfoBar.IsOpen = false;
            SuccessPlaylistCreationInfoBar.IsOpen = false;

            Search search = new Search();
            if (await search.CreatePlaylist())
            {
                Debug.WriteLine("Playlist created successfully.");
                SuccessPlaylistCreationInfoBar.IsOpen = true;
            }
            else
            {
                Debug.WriteLine("Failed to create playlist.");
                InvalidPlaylistNameInfoBar.IsOpen = true;
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
