using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace LocalToSpotify
{
    public class MusicInfo
    {
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Filepath { get; }
        public ImageSource MusicCover { get; }

        public string QueryTitle { get; set; }
        public string QueryArtist { get; set; }
        public string QueryAlbum { get; set; }

        public MusicInfo(string title, string artist, string album, string filepath, ImageSource musicCover)
        {
            Title = title;
            Artist = artist;
            Album = album;
            Filepath = filepath;
            MusicCover = musicCover;

            ConvertMetadataToQueryable();
        }

        internal void ConvertMetadataToQueryable()
        {
            QueryTitle = Title.Replace(' ', '+');
            QueryArtist = Artist.Replace(' ', '+');
            QueryAlbum = Album.Replace(' ', '+');
        }
    }

    public class MusicList
    {
        public List<MusicInfo> Songs { get; }
        public MusicList()
        {
            Songs = new List<MusicInfo>();
        }
        public void AddFile(MusicInfo file)
        {
            Songs.Add(file);
        }
    }
}
