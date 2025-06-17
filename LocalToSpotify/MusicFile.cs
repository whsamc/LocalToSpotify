using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace LocalToSpotify
{
    // This inheritance allows this class to data bind
    public class MusicFile : BindableObject
    {
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Filepath { get; }
        // public Picture? Cover { get; }

        public MusicFile(string title, string artist, string album,  string filepath)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            // this.Cover = cover;
            this.Filepath = filepath;
        }
    }
}
