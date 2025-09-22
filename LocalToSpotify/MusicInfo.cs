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
        // public Picture? Cover { get; }

        public MusicInfo(string title, string artist, string album, string filepath)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            // this.Cover = cover;
            this.Filepath = filepath;
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
