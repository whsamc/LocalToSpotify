using System.Text;
using TagLib;

namespace LocalToSpotify
{
    public partial class MainPage : ContentPage
    {
        string fileDirectory { get; set; }

        string fileName { get; set; }

        StringBuilder sb = new();
        List<MusicFile> musicList = new();
        List<string> fileList = new();

        // Get the music file paths for the folder
        private void ReadThroughFiles(object sender, EventArgs e)
        {
            // trim quotation marks
            fileDirectory = fileDirectory.Trim('"');

            // get file paths for all music files inside folder
            var musicFilePathList = Directory.GetFiles(fileDirectory, "*", SearchOption.AllDirectories);

            // Iterate through list and parse metadata from each filepath
            foreach(var musicFilePath in musicFilePathList)
            {
                ParseMetadata(musicFilePath);
            }
        }

        // Parse the metadata for the music file
        MusicFile ReadMusicFile(string filePath)
        {
            // trim the string of quotation marks
            var path = filePath.Trim('"');

            // Try to create a MusicFile object with metadata properties
            try
            {
                // File to open
                var file = TagLib.File.Create(@path);

                // Create musicfile object
                MusicFile thisSong = new MusicFile(file.Tag.Title, file.Tag.FirstAlbumArtist, file.Tag.Album);

                return thisSong;
            }

            // Catch errors
            catch(UnsupportedFormatException e)
            {
                return new MusicFile("", "", "");
            }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        // Parse the music file for the metadata
        private void ParseMetadata(string filePath)
        {
            // Run the function to read music file metadata and assign
            var song = ReadMusicFile(filePath);

            // Display song information
            musicTitleTextCell.Detail = song.Title;
            musicArtistTextCell.Detail = song.Artist;
            musicAlbumTextCell.Detail = song.Album;
        }

        // Switch pages to the Spotify Authentication page
        async private void SpotifyAuthPageButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Clicked on Spotify Login");
            await Navigation.PushAsync(new SpotifyAuth(), true);
        }

        // Set the filedirectory string whenever the entry text box is changed
        private void ReadFileDirectoryPath(object sender, TextChangedEventArgs e)
        {
            // Assign new text to string
            fileDirectory = e.NewTextValue;
        }
    }
}