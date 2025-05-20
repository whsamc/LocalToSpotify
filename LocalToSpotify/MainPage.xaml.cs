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

        MusicFile ReadMusicFile(string filePath)
        {
            var path = filePath.Trim('"');

            try
            {
                // File to open
                var file = TagLib.File.Create(@path);

                // Create musicfile object
                MusicFile thisSong = new MusicFile(file.Tag.Title, file.Tag.FirstAlbumArtist, file.Tag.Album);

                return thisSong;
            }
            catch(UnsupportedFormatException e)
            {
                return new MusicFile("", "", "");
            }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnScanClicked(object sender, EventArgs e)
        {
            var song = ReadMusicFile(musicFileInput.Text);

            musicTitleTextCell.Detail = song.Title;
            musicArtistTextCell.Detail = song.Artist;
            musicAlbumTextCell.Detail = song.Album;
        }

        async private void SpotifyAuthPageButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Clicked on Spotify Login");
            await Navigation.PushAsync(new SpotifyAuth(), true);
        }

    }

}
