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
            Debug.WriteLine("FindInSpotify called");
            ReadThroughFiles();

            Debug.WriteLine("Finished Reading through files and searching in Spotify");
            // Clear the previous search results from the UI
            GridDisplayView1.Items.Clear();
            GridDisplayView2.Items.Clear();

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

            try
            {
                Search search = new Search();
                
                Debug.WriteLine("Iterating through MusicList and searching Spotify for each song...");
                foreach (MusicInfo musicInfo in MusicList)
                {
                    if(Data.SpotifyToken != null)
                    {
                        // Search using spotify api and return its search results
                        var searchResults = await search.SearchSong(Data.SpotifyToken, musicInfo);
                        if (searchResults != null)
                        {
                            Debug.WriteLine($"Search results for {musicInfo.Title}: {searchResults.tracks.items.Count} items found.");
                            for (int i = 0; i < 3; i++)
                            {
                                Debug.WriteLine($"Item {i + 1}: {searchResults.tracks.items[i].name} by {string.Join(", ", searchResults.tracks.items[i].artists.Select(a => a.name))}");
                            }

                            // Display the search result in the UI
                            string title = searchResults.tracks.items[0].name;
                            string artist = searchResults.tracks.items[0].artists[0].name;
                            string album = searchResults.tracks.items[0].album.name;
                            
                            DisplayUI(musicInfo, title, artist, album);
                            //DisplaySearchResults(musicInfo, title, artist, album);

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

                var image = file.Tag.Pictures.First().Data;
                MemoryStream ms = new MemoryStream(image.Data);
                ms.Write(image.Data, 0, image.Data.Length);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(ms.AsRandomAccessStream());
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

        private void DisplayUI(MusicInfo musicFile, string title, string artist, string album)
        {
            try
            {
                StackPanel searchResultUIValues = new StackPanel();

                GridDisplayView1.Items.Add(musicFile);
                GridDisplayView2.Items.Add(searchResultUIValues);

                // Title Value Block
                TextBlock textBlockTitleValue = new TextBlock();
                textBlockTitleValue.Text = $"{title}";
                textBlockTitleValue.IsTextSelectionEnabled = true;
                textBlockTitleValue.Margin = new Thickness(0, 5, 0, 0);
                textBlockTitleValue.HorizontalAlignment.Equals(HorizontalAlignment.Left);
                // Artist Value Block
                TextBlock textBlockArtistValue = new TextBlock();
                textBlockArtistValue.Text = $"{artist}";
                textBlockArtistValue.IsTextSelectionEnabled = true;
                textBlockArtistValue.Margin = new Thickness(0, 5, 0, 0);
                textBlockArtistValue.HorizontalAlignment.Equals(HorizontalAlignment.Left);
                // Album Value Block
                TextBlock textBlockAlbumValue = new TextBlock();
                textBlockAlbumValue.Text = $"{album}";
                textBlockAlbumValue.IsTextSelectionEnabled = true;
                textBlockAlbumValue.Margin = new Thickness(0, 5, 0, 20);
                textBlockAlbumValue.HorizontalAlignment.Equals(HorizontalAlignment.Left);

                searchResultUIValues.Children.Add(textBlockTitleValue);
                searchResultUIValues.Children.Add(textBlockArtistValue);
                searchResultUIValues.Children.Add(textBlockAlbumValue);

                Grid.SetRow(textBlockTitleValue, 0);
                Grid.SetRow(textBlockArtistValue, 1);
                Grid.SetRow(textBlockAlbumValue, 2);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error displaying search results for {title} by {artist}: {ex.Message}");
            }
        }

        // Method to add display elements for search results
        private void DisplaySearchResults(MusicInfo musicFile, string title, string artist, string album)
        {
            try
            {
                Debug.WriteLine("Creating textblocks in new Grid to display search results...");

                StackPanel searchResultUIHeaders = new StackPanel();
                StackPanel searchResultUIValues = new StackPanel();


                GridDisplayView1.Items.Add(searchResultUIHeaders);
                GridDisplayView2.Items.Add(searchResultUIValues);

                // Set the alignment for the grids
                searchResultUIHeaders.HorizontalAlignment = HorizontalAlignment.Right;
                searchResultUIValues.HorizontalAlignment = HorizontalAlignment.Left;

                // Title Header Block
                TextBlock textBlockTitleHeader = new TextBlock();
                textBlockTitleHeader.Text = $"Title: {musicFile.Title}";
                textBlockTitleHeader.IsTextSelectionEnabled = true;
                textBlockTitleHeader.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
                textBlockTitleHeader.Margin = new Thickness(0, 5, 0, 0);
                textBlockTitleHeader.HorizontalAlignment.Equals(HorizontalAlignment.Right);
                textBlockTitleHeader.TextAlignment.Equals(TextAlignment.Right);
                // Title Value Block
                TextBlock textBlockTitleValue = new TextBlock();
                textBlockTitleValue.Text = $"{title}";
                textBlockTitleValue.IsTextSelectionEnabled = true;
                textBlockTitleValue.Margin = new Thickness(0, 5, 0, 0);
                textBlockTitleValue.HorizontalAlignment.Equals(HorizontalAlignment.Left);

                // Artist Header Block
                TextBlock textBlockArtistHeader = new TextBlock();
                textBlockArtistHeader.Text = $"Artist: {musicFile.Artist}";
                textBlockArtistHeader.IsTextSelectionEnabled = true;
                textBlockArtistHeader.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
                textBlockArtistHeader.Margin = new Thickness(0, 5, 0, 0);
                textBlockArtistHeader.HorizontalAlignment.Equals(HorizontalAlignment.Right);
                // Artist Value Block
                TextBlock textBlockArtistValue = new TextBlock();
                textBlockArtistValue.Text = $"{artist}";
                textBlockArtistValue.IsTextSelectionEnabled = true;
                textBlockArtistValue.Margin = new Thickness(0, 5, 0, 0);
                textBlockArtistValue.HorizontalAlignment.Equals(HorizontalAlignment.Left);


                // Album Header Block
                TextBlock textBlockAlbumHeader = new TextBlock();
                textBlockAlbumHeader.Text = $"Album: {musicFile.Album}";
                textBlockAlbumHeader.IsTextSelectionEnabled = true;
                textBlockAlbumHeader.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
                textBlockAlbumHeader.Margin = new Thickness(0, 5, 0, 20);
                textBlockArtistHeader.HorizontalAlignment.Equals(HorizontalAlignment.Right);
                // Album Value Block
                TextBlock textBlockAlbumValue = new TextBlock();
                textBlockAlbumValue.Text = $"{album}";
                textBlockAlbumValue.IsTextSelectionEnabled = true;
                textBlockAlbumValue.Margin = new Thickness(0, 5, 0, 20);
                textBlockAlbumValue.HorizontalAlignment.Equals(HorizontalAlignment.Left);


                // Add TextBlocks to the visual tree
                searchResultUIHeaders.Children.Add(textBlockTitleHeader);
                searchResultUIHeaders.Children.Add(textBlockArtistHeader);
                searchResultUIHeaders.Children.Add(textBlockAlbumHeader);

                Grid.SetRow(textBlockTitleHeader, 0);
                Grid.SetRow(textBlockArtistHeader, 1);
                Grid.SetRow(textBlockAlbumHeader, 2);

                searchResultUIValues.Children.Add(textBlockTitleValue);
                searchResultUIValues.Children.Add(textBlockArtistValue);
                searchResultUIValues.Children.Add(textBlockAlbumValue);

                Grid.SetRow(textBlockTitleValue, 0);
                Grid.SetRow(textBlockArtistValue, 1);
                Grid.SetRow(textBlockAlbumValue, 2);

                // InitializeComponent();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error displaying search results for {title} by {artist}: {ex.Message}");
            }
        }
    }
}
