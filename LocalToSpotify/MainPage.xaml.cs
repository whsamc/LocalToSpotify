﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.IO;
using TagLib;

namespace LocalToSpotify
{
    public partial class MainPage : ContentPage
    {
        string? FileDirectory;

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

        public MainPage()
        {
            InitializeComponent();

            BindingContext = this;

            Debug.WriteLine("Testing");
        }

        // Get the music file paths for the folder
        private void ReadThroughFiles(object sender, EventArgs e)
        {
            // Return if FileDirectory is null
            if(FileDirectory == null) return;

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
            foreach(var MusicFilePath in MusicFilePathList)
            {
                // Check the file type with fileExtensions hashset
                if(fileExtensions.Contains(Path.GetExtension(MusicFilePath)))
                {
                    AddToMusicList(ParseMusicFile(MusicFilePath));
                }
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
            MusicList.Add(song);
        }

        // Switch pages to the Spotify Authentication page
        async private void SpotifyAuthPageButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Clicked on Spotify Login");
            await Navigation.PushAsync(new SpotifyAuth(), true);
        }

        // Set the FileDirectory string whenever the entry text box is changed
        private void ReadFileDirectoryPath(object sender, TextChangedEventArgs e)
        {
            // Assign new text to string
            FileDirectory = e.NewTextValue;
        }
    }
}