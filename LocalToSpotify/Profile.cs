using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalToSpotify
{
    public class Profile
    {
        public string? display_name { get; set; }

        public Profile()
        {
            display_name = "empty"; // Default display name
        }
    }
}
