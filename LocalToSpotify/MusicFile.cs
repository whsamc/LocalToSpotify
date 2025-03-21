using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    class MusicFile
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }

        public MusicFile(string title, string artist, string album)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
        }
    }
}
