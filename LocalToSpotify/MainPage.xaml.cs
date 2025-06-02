using System.Text;
using TagLib;

namespace LocalToSpotify
{
    public partial class MainPage : ContentPage
    {
        string fileDirectory { get; set; }

        public static List<MusicFile> musicList = new List<MusicFile>();

        List<string> musicFilePathList = new List<string>();

        public MainPage()
        {
            InitializeComponent();

            // Bind the collectionview(target) to musicList(source)
            songsScrollView.BindingContext = musicList;
        }

        // Get the music file paths for the folder
        private void ReadThroughFiles(object sender, EventArgs e)
        {
            // Return if fileDirectory is null
            if(fileDirectory == null) return;

            musicFilePathList.Clear();
            musicList.Clear();

            // trim quotation marks
            fileDirectory = fileDirectory.Trim('"');

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
                AddToMusicList(ParseMusicFile(musicFilePath));
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
            catch(UnsupportedFormatException e)
            {
                return new MusicFile("", "", "", filePath);
            }
        }

        private void AddToMusicList(MusicFile song)
        {
            musicList.Add(song);
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