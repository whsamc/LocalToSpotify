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
        public MainPage()
        {
            InitializeComponent();


        }

        // Get the music file paths for the folder
        private void ReadThroughFiles(object sender, EventArgs e)
        {
            // Return if fileDirectory is null
            if(fileDirectory == null) return;

            // trim quotation marks
            fileDirectory = fileDirectory.Trim('"');

            List<string> musicFilePathList = new List<string>();

            // If the path is actually just a file
            if (System.IO.File.Exists(fileDirectory))
            {
                // Add single item to the list because of the parse method later
                musicFilePathList.Add(fileDirectory);
            }
            // If the path is a directory
            else
            {
                // get file paths for all music files inside folder
                musicFilePathList = Directory.GetFiles(fileDirectory, "*", SearchOption.AllDirectories).ToList();
            }

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

            // Catch wrong format exceptions
            catch(UnsupportedFormatException e)
            {
                return new MusicFile("", "", "");
            }
        }

        // Parse the music file for the metadata
        private void ParseMetadata(string filePath)
        {
            // Run the function to read music file metadata and assign
            var song = ReadMusicFile(filePath);

            // Display song information
            musicTitleTextCell.Text = song.Title;
            musicArtistTextCell.Text = song.Artist;
            musicAlbumTextCell.Text = song.Album;
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