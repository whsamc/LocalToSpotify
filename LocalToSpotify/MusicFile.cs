using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    // This inheritance allows this class to data bind
    public class MusicFile
    {
        public string Title;
        public string Artist;
        public string Album;
        public string Filepath;

        public MusicFile(string title, string artist, string album, string filepath)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            this.Filepath = filepath;
        }
    }

    public class MusicDirectory : IEnumerable
    {
        private MusicFile[] _musicDirectory;

        public MusicDirectory(MusicFile[] musicDirectoryArray)
        {
            _musicDirectory = new MusicFile[musicDirectoryArray.Length];

            for (int i = 0; i < musicDirectoryArray.Length; i++)
            {
                musicDirectoryArray[i] = musicDirectoryArray[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public MusicDirectoryEnum GetEnumerator()
        {
            return new MusicDirectoryEnum(_musicDirectory);
        }
    }

    public class MusicDirectoryEnum : IEnumerator
    {
        public MusicFile[] _musicDirectory;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public MusicDirectoryEnum(MusicFile[] list)
        {
            _musicDirectory = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _musicDirectory.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public MusicFile Current
        {
            get
            {
                try
                {
                    return _musicDirectory[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
