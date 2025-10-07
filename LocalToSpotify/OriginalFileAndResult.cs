using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    class OriginalFileAndResult
    {
        public MusicInfo originalFile { get; set; }
        public SpotifySearchResponse result {get; set;}

        public OriginalFileAndResult(MusicInfo originalFile, SpotifySearchResponse result)
        {
            this.originalFile = originalFile;
            this.result = result;
        }
    }
}
