using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace LocalToSpotify
{
    internal class MusicFile
    {
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Filepath { get; }
        // public Picture? Cover { get; }

        public MusicFile(string title, string artist, string album, string filepath)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            // this.Cover = cover;
            this.Filepath = filepath;
        }
    }
}
