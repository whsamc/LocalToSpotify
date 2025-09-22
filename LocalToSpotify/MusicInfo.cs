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

        public string QueryTitle { get; set; }
        public string QueryArtist { get; set; }
        public string QueryAlbum { get; set; }
        // public Picture? Cover { get; }

        public MusicInfo(string title, string artist, string album, string filepath)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            // this.Cover = cover;
            this.Filepath = filepath;

            ConvertMetadataToQueryable();
        }

        internal void ConvertMetadataToQueryable()
        {
            this.QueryTitle = Title.Replace(' ', '+');
            this.QueryArtist = Artist.Replace(' ', '+');
            this.QueryAlbum = Album.Replace(' ', '+');
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
