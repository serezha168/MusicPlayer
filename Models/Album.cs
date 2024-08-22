// Models/Album.cs
using System.Collections.Generic;

namespace MusicPlayer.Models
{
    public class Album
    {
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public List<Track> Tracks { get; set; } = new List<Track>();
    }
}
